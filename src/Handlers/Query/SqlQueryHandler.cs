using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query.Response;
using System.Data.Common;
using static Dapper.SqlMapper;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query;

/// <summary>
/// Executes SQL query operations using a shared orchestrator pipeline.
/// </summary>
public sealed class SqlQueryHandler : ISqlQueryHandler
{
    private readonly ISqlExecutionOrchestrator _executor;

    /// <summary>
    /// Creates a new query handler that delegates execution to the orchestrator.
    /// </summary>
    public SqlQueryHandler(ISqlExecutionOrchestrator executor)
    {
        _executor = executor ??
            throw new ArgumentNullException(nameof(executor));
    }

    /// <summary>
    /// Executes a SQL query and returns all rows.
    /// </summary>
    public Task<IEnumerable<TDbEntity>> QueryAsync<TDbEntity>(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken) =>
        _executor.ExecuteAsync(
            sql,
            transaction,
            options,
            (tx, cmd) => tx.Connection!.QueryAsync<TDbEntity>(cmd),
            cancellationToken
        );

    /// <summary>
    /// Executes a SQL query and returns exactly one row.
    /// </summary>
    public Task<TDbEntity> QuerySingleAsync<TDbEntity>(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken) =>
        _executor.ExecuteAsync(
            sql,
            transaction,
            options,
            (tx, cmd) => tx.Connection!.QuerySingleAsync<TDbEntity>(cmd),
            cancellationToken
        );

    /// <summary>
    /// Executes a SQL query and returns one row or null.
    /// </summary>
    public Task<TDbEntity?> QuerySingleOrDefaultAsync<TDbEntity>(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken) =>
        _executor.ExecuteAsync(
            sql,
            transaction,
            options,
            (tx, cmd) => tx.Connection!.QuerySingleOrDefaultAsync<TDbEntity>(cmd),
            cancellationToken
        );

    /// <summary>
    /// Executes a SQL query and returns the first row.
    /// </summary>
    public Task<TDbEntity> QueryFirstAsync<TDbEntity>(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken) =>
        _executor.ExecuteAsync(
            sql,
            transaction,
            options,
            (tx, cmd) => tx.Connection!.QueryFirstAsync<TDbEntity>(cmd),
            cancellationToken
        );

    /// <summary>
    /// Executes a SQL query and returns the first row or null.
    /// </summary>
    public Task<TDbEntity?> QueryFirstOrDefaultAsync<TDbEntity>(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken) =>
        _executor.ExecuteAsync(
            sql,
            transaction,
            options,
            (tx, cmd) => tx.Connection!.QueryFirstOrDefaultAsync<TDbEntity>(cmd),
            cancellationToken
        );

    /// <summary>
    /// Executes a SQL query that returns multiple result sets.
    /// </summary>
    public async Task<IDbGridReader> QueryMultipleAsync(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        CancellationToken cancellationToken)
    {
        GridReader? gridReader =
            await _executor.ExecuteAsync(
                sql,
                transaction,
                options,
                (tx, cmd) => tx.Connection!.QueryMultipleAsync(cmd),
                cancellationToken
            );

        return new DatabaseGridReader(gridReader);
    }
}
