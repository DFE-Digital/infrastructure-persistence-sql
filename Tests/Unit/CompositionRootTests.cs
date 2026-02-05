using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Command;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Factories;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Options;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Connection;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests;

public sealed class TestDbName : IDbName { }

public class CompositionRootTests
{
    private sealed class FakeConnection : DbConnection
    {
        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => "Fake";
        public override string DataSource => "Fake";
        public override string ServerVersion => "1.0";
        public override ConnectionState State => ConnectionState.Open;
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => null!;
        public override void ChangeDatabase(string databaseName) { }
        public override void Close() { }
        public override void Open() { }
        protected override DbCommand CreateDbCommand() => null!;
    }

    private static DbConnection FakeFactory(IServiceProvider _) => new FakeConnection();

    [Fact]
    public void AddDatabase_Registers_All_Expected_Services()
    {
        var services = new ServiceCollection();

        services.AddDatabase<TestDbName>(FakeFactory);

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<ISqlCommandExecutor>());
        Assert.NotNull(provider.GetService<ISqlParameterRedactor>());
        Assert.NotNull(provider.GetService<ISqlExecutionOrchestrator>());
        Assert.NotNull(provider.GetService<ISqlCommandHandler>());
        Assert.NotNull(provider.GetService<ISqlQueryHandler>());
        Assert.NotNull(provider.GetService<ICommandDefinitionFactory>());
        Assert.NotNull(provider.GetService<DbContextProvider<TestDbName>>());
        Assert.NotNull(provider.GetService<IDbContextProvider>());
        Assert.NotNull(provider.GetService<DbConnectionFactory<TestDbName>>());
        Assert.NotNull(provider.GetService<DatabaseOptions>());
    }

    [Fact]
    public void AddDatabaseAsDefault_Registers_Default_Handlers()
    {
        var services = new ServiceCollection();

        services.AddDatabaseAsDefault<TestDbName>(FakeFactory);

        var provider = services.BuildServiceProvider();

        Assert.IsType<SqlCommandHandler>(provider.GetRequiredService<ISqlCommandHandler>());
        Assert.IsType<SqlQueryHandler>(provider.GetRequiredService<ISqlQueryHandler>());
        Assert.IsType<DbContextProvider<TestDbName>>(provider.GetRequiredService<IDbContextProvider>());
    }

    [Fact]
    public void AddDatabase_StringName_Creates_Runtime_Type_And_Registers_Services()
    {
        var services = new ServiceCollection();

        services.AddDatabase("MyRuntimeDb", FakeFactory);

        var provider = services.BuildServiceProvider();

        // Should resolve DatabaseOptions
        Assert.NotNull(provider.GetService<DatabaseOptions>());

        // Should resolve IDbContextProvider
        Assert.NotNull(provider.GetService<IDbContextProvider>());
    }

    [Fact]
    public void AddDatabaseAsDefault_StringName_Registers_Default_Handlers()
    {
        var services = new ServiceCollection();

        services.AddDatabaseAsDefault("MyRuntimeDb", FakeFactory);

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IDbContextProvider>());
        Assert.IsType<SqlCommandHandler>(provider.GetRequiredService<ISqlCommandHandler>());
        Assert.IsType<SqlQueryHandler>(provider.GetRequiredService<ISqlQueryHandler>());
    }

    [Fact]
    public void AddDatabase_Registers_ConnectionFactory_That_Creates_Connections()
    {
        var services = new ServiceCollection();

        services.AddDatabase<TestDbName>(FakeFactory);

        var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<DbConnectionFactory<TestDbName>>();

        DbConnection conn = factory();

        Assert.IsType<FakeConnection>(conn);
    }
}
