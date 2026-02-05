using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Command;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Options;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Connection;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Context;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper;

/// <summary>
/// Provides extension methods for registering strongly-typed database services
/// used by the Dapper-based persistence layer.
/// </summary>
public static class CompositionRoot
{
    /// <summary>
    /// Registers all required services for a strongly-typed database context,
    /// including connection factories, handlers, orchestrators, and options.
    /// </summary>
    public static IServiceCollection AddDatabase<TName>(
        this IServiceCollection services,
        Func<IServiceProvider, DbConnection> connectionFactory,
        Action<DatabaseOptions>? config = null
    ) where TName : IDbName
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionFactory);

        // Ensure logging is available for orchestrators, handlers, and context providers
        services.AddLogging();

        // Ensure IOptions<DatabaseOptions> is always available
        services.AddOptions<DatabaseOptions>();

        if (config is not null)
        {
            services.Configure(config);
        }

        // Register DatabaseOptions (resolved from IOptions<T>)
        services.TryAddSingleton(s =>
            s.GetRequiredService<IOptions<DatabaseOptions>>().Value);

        // Register the command executor
        services.TryAddScoped<ISqlCommandExecutor, SqlCommandExecutor>();

        // Register parameter redactor
        services.TryAddSingleton<ISqlParameterRedactor, SqlParameterRedactor>();

        // Register connection factory
        services.TryAddTransient<DbConnectionFactory<TName>>(s =>
            () => connectionFactory(s));

        // Register orchestrator + handlers
        services.TryAddScoped<ISqlExecutionOrchestrator, SqlExecutionOrchestrator>();
        services.TryAddScoped<ISqlCommandHandler, SqlCommandHandler>();
        services.TryAddScoped<ISqlQueryHandler, SqlQueryHandler>();
        services.TryAddScoped<ICommandDefinitionFactory, CommandDefinitionFactory>();

        // Register DbContext provider
        services.TryAddScoped<IDbContextProvider, DbContextProvider<TName>>();
        services.TryAddScoped<DbContextProvider<TName>, DbContextProvider<TName>>();

        return services;
    }

    /// <summary>
    /// Registers the strongly-typed database services and also configures them
    /// as the default database context for the application.
    /// </summary>
    public static IServiceCollection AddDatabaseAsDefault<TName>(
        this IServiceCollection services,
        Func<IServiceProvider, DbConnection> connectionFactory,
        Action<DatabaseOptions>? config = null
    ) where TName : IDbName
    {
        services.AddDatabase<TName>(connectionFactory, config);

        // Register default DbContextProvider + handlers
        services.TryAddScoped<IDbContextProvider, DbContextProvider<TName>>();
        services.TryAddScoped<ISqlCommandHandler, SqlCommandHandler>();
        services.TryAddScoped<ISqlQueryHandler, SqlQueryHandler>();

        return services;
    }

    /// <summary>
    /// Registers database services using a runtime-generated database name type.
    /// This enables string-based registration without requiring a manually defined <see cref="IDbName"/> type.
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        string databaseName,
        Func<IServiceProvider, DbConnection> connectionFactory,
        Action<DatabaseOptions>? config = null)
    {
        ArgumentNullException.ThrowIfNull(databaseName);

        Type dbType = CreateRuntimeDbName(databaseName);

        MethodInfo method = typeof(CompositionRoot)
            .GetMethods()
            .Single(method =>
                method.Name == nameof(AddDatabase) &&
                method.IsGenericMethod);

        MethodInfo generic = method.MakeGenericMethod(dbType);

        generic.Invoke(null, [services, connectionFactory, config]);

        return services;
    }

    /// <summary>
    /// Registers database services as the default database using a runtime-generated
    /// <see cref="IDbName"/> implementation based on the provided string name.
    /// </summary>
    public static IServiceCollection AddDatabaseAsDefault(
        this IServiceCollection services,
        string databaseName,
        Func<IServiceProvider, DbConnection> connectionFactory,
        Action<DatabaseOptions>? config = null)
    {
        ArgumentNullException.ThrowIfNull(databaseName);

        Type dbType = CreateRuntimeDbName(databaseName);

        MethodInfo method = typeof(CompositionRoot)
            .GetMethods()
            .Single(method =>
                method.Name == nameof(AddDatabaseAsDefault) &&
                method.IsGenericMethod);

        MethodInfo generic = method.MakeGenericMethod(dbType);

        generic.Invoke(null, [services, connectionFactory, config]);

        return services;
    }

    /// <summary>
    /// Cache of dynamically generated <see cref="IDbName"/> types.
    /// Ensures that repeated calls using the same database name return the same type,
    /// preventing duplicate type name exceptions within the dynamic assembly.
    /// </summary>
    private static readonly Dictionary<string, Type> RuntimeTypes = new();

    /// <summary>
    /// Creates (or retrieves from cache) a runtime-generated type that implements <see cref="IDbName"/>.
    /// This prevents duplicate type name exceptions when the same database name is used multiple times.
    /// </summary>
    /// <param name="name">The logical name of the database.</param>
    /// <returns>A unique <see cref="Type"/> implementing <see cref="IDbName"/>.</returns>
    private static Type CreateRuntimeDbName(string name)
    {
        // If a type for this name already exists, reuse it
        if (RuntimeTypes.TryGetValue(name, out var existing))
            return existing;

        // Otherwise create a new runtime type
        TypeBuilder typeBuilder = Module.DefineType(
            name,
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);

        typeBuilder.AddInterfaceImplementation(typeof(IDbName));

        Type generatedType = typeBuilder.CreateType()!;

        // Cache the generated type for future reuse
        RuntimeTypes[name] = generatedType;

        return generatedType;
    }

    /// <summary>
    /// Constants used for dynamic database type generation.
    /// </summary>
    private static class RuntimeDbNameConstants
    {
        public const string AssemblyName = "DynamicDbNames";
        public const string ModuleName = "Main";
    }

    /// <summary>
    /// Shared dynamic module used for emitting runtime-generated <see cref="IDbName"/> types.
    /// </summary>
    private static readonly ModuleBuilder Module =
        AssemblyBuilder
            .DefineDynamicAssembly(
                new AssemblyName(RuntimeDbNameConstants.AssemblyName),
                AssemblyBuilderAccess.Run)
            .DefineDynamicModule(RuntimeDbNameConstants.ModuleName);
}
