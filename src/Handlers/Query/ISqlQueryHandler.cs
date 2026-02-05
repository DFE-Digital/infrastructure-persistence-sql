using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query.Response;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query
{
    /// <summary>
    /// Defines operations for executing SQL queries and returning mapped results.
    /// </summary>
    public interface ISqlQueryHandler
    {
        /// <summary>
        /// Executes a SQL query and returns all rows.
        /// </summary>
        /// <typeparam name="TDbEntity">The type to map each row to.</typeparam>
        /// <param name="commandText">The SQL query text.</param>
        /// <param name="transaction">
        /// The active transaction whose associated connection will be used.
        /// </param>
        /// <param name="options">Execution options including parameters and timeout.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        Task<IEnumerable<TDbEntity>> QueryAsync<TDbEntity>(
            string commandText,
            DbTransaction transaction,
            SqlRequestOptions options,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a SQL query and returns exactly one row.
        /// </summary>
        /// <typeparam name="TDbEntity">The expected result type.</typeparam>
        /// <param name="commandText">The SQL query text.</param>
        /// <param name="transaction">
        /// The active transaction whose associated connection will be used.
        /// </param>
        /// <param name="options">Execution options including parameters and timeout.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        Task<TDbEntity> QuerySingleAsync<TDbEntity>(
            string commandText,
            DbTransaction transaction,
            SqlRequestOptions options,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a SQL query and returns one row or null.
        /// </summary>
        /// <typeparam name="TDbEntity">The expected result type.</typeparam>
        /// <param name="commandText">The SQL query text.</param>
        /// <param name="transaction">
        /// The active transaction whose associated connection will be used.
        /// </param>
        /// <param name="options">Execution options including parameters and timeout.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        Task<TDbEntity?> QuerySingleOrDefaultAsync<TDbEntity>(
            string commandText,
            DbTransaction transaction,
            SqlRequestOptions options,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a SQL query and returns the first row.
        /// </summary>
        /// <typeparam name="TDbEntity">The expected result type.</typeparam>
        /// <param name="commandText">The SQL query text.</param>
        /// <param name="transaction">
        /// The active transaction whose associated connection will be used.
        /// </param>
        /// <param name="options">Execution options including parameters and timeout.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        Task<TDbEntity> QueryFirstAsync<TDbEntity>(
            string commandText,
            DbTransaction transaction,
            SqlRequestOptions options,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a SQL query and returns the first row or null.
        /// </summary>
        /// <typeparam name="TDbEntity">The expected result type.</typeparam>
        /// <param name="commandText">The SQL query text.</param>
        /// <param name="transaction">
        /// The active transaction whose associated connection will be used.
        /// </param>
        /// <param name="options">Execution options including parameters and timeout.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        Task<TDbEntity?> QueryFirstOrDefaultAsync<TDbEntity>(
            string commandText,
            DbTransaction transaction,
            SqlRequestOptions options,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes a SQL query that returns multiple result sets.
        /// </summary>
        /// <param name="commandText">The SQL query text.</param>
        /// <param name="transaction">
        /// The active transaction whose associated connection will be used.
        /// </param>
        /// <param name="options">Execution options including parameters and timeout.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        Task<IDbGridReader> QueryMultipleAsync(
            string commandText,
            DbTransaction transaction,
            SqlRequestOptions options,
            CancellationToken cancellationToken);
    }
}
