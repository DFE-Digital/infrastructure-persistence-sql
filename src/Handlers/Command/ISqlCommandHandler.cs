using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Command;

/// <summary>
/// Defines operations for executing SQL commands such as non‑query, scalar,
/// and reader‑based executions.
/// </summary>
public interface ISqlCommandHandler
{
    /// <summary>
    /// Executes a non‑query SQL command (INSERT, UPDATE, DELETE).
    /// </summary>
    /// <param name="commandText">The SQL command text.</param>
    /// <param name="transaction">
    /// The active transaction whose associated connection will be used.
    /// </param>
    /// <param name="options">Execution options including parameters and timeout.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    Task<int> ExecuteAsync(
        string commandText,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Executes a SQL command and returns a single scalar value.
    /// </summary>
    /// <typeparam name="TDbEntity">The expected scalar result type.</typeparam>
    /// <param name="commandText">The SQL command text.</param>
    /// <param name="transaction">
    /// The active transaction whose associated connection will be used.
    /// </param>
    /// <param name="options">Execution options including parameters and timeout.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    Task<TDbEntity?> ExecuteScalarAsync<TDbEntity>(
        string commandText,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Executes a SQL command and returns a data reader for streaming results.
    /// </summary>
    /// <param name="queryText">The SQL query text.</param>
    /// <param name="transaction">
    /// The active transaction whose associated connection will be used.
    /// </param>
    /// <param name="options">Execution options including parameters and timeout.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    Task<DbDataReader> ExecuteReaderAsync(
        string queryText,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken
    );
}
