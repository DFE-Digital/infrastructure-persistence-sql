using Dapper;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Factories;

/// <summary>
/// Defines a factory interface for creating <see cref="CommandDefinition"/> instances
/// used in Dapper-based SQL operations.
/// </summary>
public interface ICommandDefinitionFactory
{
    /// <summary>
    /// Creates a <see cref="CommandDefinition"/> for executing a SQL command using Dapper.
    /// </summary>
    /// <param name="commandText">The SQL command text to execute.</param>
    /// <param name="transaction">The database transaction context, if any.</param>
    /// <param name="options">Custom options for SQL request execution, such as timeouts or buffering.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A configured <see cref="CommandDefinition"/> ready for execution.</returns>
    CommandDefinition CreateCommandDefinition(
        string commandText,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken token
    );
}