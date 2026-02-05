using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Command;

/// <summary>
/// Executes SQL command operations using a shared orchestrator pipeline.
/// </summary>
public sealed class SqlCommandHandler : ISqlCommandHandler
{
    private readonly ISqlExecutionOrchestrator _executor;

    /// <summary>
    /// Creates a new command handler that delegates execution to the orchestrator.
    /// </summary>
    /// <param name="executor">The orchestrator responsible for executing SQL operations.</param>
    public SqlCommandHandler(ISqlExecutionOrchestrator executor)
    {
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    /// <summary>
    /// Executes a non‑query SQL command (INSERT, UPDATE, DELETE).
    /// </summary>
    public Task<int> ExecuteAsync(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken) =>
        _executor.ExecuteAsync(
            sql,
            transaction,
            options,
            (tx, cmd) => tx.Connection!.ExecuteAsync(cmd),
            cancellationToken
        );

    /// <summary>
    /// Executes a SQL command and returns a single scalar value.
    /// </summary>
    public Task<TDbEntity?> ExecuteScalarAsync<TDbEntity>(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken) =>
        _executor.ExecuteAsync(
            sql,
            transaction,
            options,
            (tx, cmd) => tx.Connection!.ExecuteScalarAsync<TDbEntity>(cmd),
            cancellationToken
        );

    /// <summary>
    /// Executes a SQL command and returns a <see cref="DbDataReader"/> for streaming results.
    /// </summary>
    public Task<DbDataReader> ExecuteReaderAsync(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken) =>
        _executor.ExecuteAsync(
            sql,
            transaction,
            options,
            (tx, cmd) => tx.Connection!.ExecuteReaderAsync(cmd),
            cancellationToken
        );
}
