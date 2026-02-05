using Dapper;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;

/// <summary>
/// Defines a contract for executing SQL commands using Dapper with centralized validation
/// and flexible execution strategies.
/// </summary>
/// <remarks>
/// This interface abstracts the execution of Dapper commands, allowing injection of custom
/// execution delegates (e.g., <c>QueryAsync</c>, <c>ExecuteAsync</c>) while enforcing consistent
/// input validation and cancellation support.
/// </remarks>
public interface ISqlCommandExecutor
{
    /// <summary>
    /// Executes a SQL command using a provided Dapper execution delegate.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the execution delegate.</typeparam>
    /// <param name="commandText">The SQL query to execute.</param>
    /// <param name="transaction">
    /// The active <see cref="DbTransaction"/> whose associated connection will be used.
    /// </param>
    /// <param name="options">Execution options such as buffering, timeout, and command type.</param>
    /// <param name="executorFactory">
    /// A higher‑order function that accepts a <see cref="DbTransaction"/> and returns a delegate
    /// that executes a <see cref="CommandDefinition"/> and returns a <see cref="Task{TResult}"/>.
    /// </param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution result.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="commandText"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="transaction"/> or <paramref name="options"/> is null.
    /// </exception>
    Task<TResult> ExecuteAsync<TResult>(
        string commandText,
        DbTransaction transaction,
        SqlRequestOptions options,
        Func<DbTransaction, Func<CommandDefinition, Task<TResult>>> executorFactory,
        CancellationToken cancellationToken = default
    );
}
