using Dapper;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;

/// <summary>
/// Provides a reusable execution pipeline for SQL commands and queries,
/// applying logging, redaction, cancellation checks, and consistent
/// <see cref="CommandDefinition"/> construction.
/// </summary>
public interface ISqlExecutionOrchestrator
{
    /// <summary>
    /// Executes a SQL operation using the supplied Dapper delegate.
    /// </summary>
    /// <typeparam name="TResult">The result type returned by the operation.</typeparam>
    /// <param name="sql">The SQL text to execute.</param>
    /// <param name="transaction">
    /// The active transaction whose associated connection will be used.
    /// </param>
    /// <param name="options">Execution options including parameters and timeout.</param>
    /// <param name="executor">
    /// A delegate that performs the actual Dapper call using the provided
    /// <see cref="DbTransaction"/> and <see cref="CommandDefinition"/>.
    /// </param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The result of the executed SQL operation.</returns>
    Task<TResult> ExecuteAsync<TResult>(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        Func<DbTransaction, CommandDefinition, Task<TResult>> executor,
        CancellationToken cancellationToken);
}
