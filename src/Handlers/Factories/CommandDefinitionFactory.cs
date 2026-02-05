using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Options;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Factories;

/// <summary>
/// Factory for creating <see cref="CommandDefinition"/> instances used in Dapper-based SQL execution.
/// Centralizes default configuration and ensures consistent command construction.
/// </summary>
public sealed class CommandDefinitionFactory(IOptions<DatabaseOptions> dbOptions) : ICommandDefinitionFactory
{
    private readonly DatabaseOptions _options =
        dbOptions?.Value ??
        throw new ArgumentNullException(nameof(dbOptions));

    /// <summary>
    /// Creates a configured <see cref="CommandDefinition"/> for executing a SQL command via Dapper.
    /// </summary>
    /// <param name="commandText">The SQL command text to execute.</param>
    /// <param name="transaction">An optional database transaction to associate with the command.</param>
    /// <param name="options">Request-specific options such as parameters, timeout, and command type.</param>
    /// <param name="token">A cancellation token to observe during command execution.</param>
    /// <returns>A fully constructed <see cref="CommandDefinition"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="options"/> is null or if <paramref name="options.Parameters"/> is null.
    /// </exception>
    public CommandDefinition CreateCommandDefinition(
        string commandText,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken token
    ) =>
        new(
            commandText,
            options.Parameters ?? throw new ArgumentNullException(nameof(options)),
            transaction,
            options.Timeout ?? _options.DefaultTimeout,
            options.Type,
            cancellationToken: token
        );
}