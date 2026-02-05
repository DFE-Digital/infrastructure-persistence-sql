using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Factories;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;

/// <summary>
/// Provides centralized execution logic for Dapper commands, including input validation and flexible execution delegation.
/// </summary>
/// <remarks>
/// This class encapsulates reusable patterns for executing SQL commands via Dapper, allowing injection of execution strategies
/// such as <c>QueryAsync</c>, <c>ExecuteAsync</c>, or custom delegates. It ensures inputs are validated before dispatching
/// and supports cancellation and transaction contexts. Designed for modular clarity and onboarding-friendly diagnostics.
/// </remarks>
public class SqlCommandExecutor(
    ICommandDefinitionFactory commandDefinitionFactory) : ISqlCommandExecutor
{
    /// <summary>
    /// Executes a SQL command using a provided Dapper execution delegate.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the execution delegate.</typeparam>
    /// <param name="commandText">The SQL query to execute.</param>
    /// <param name="dbTransaction">The active <see cref="DbTransaction"/> whose connection will be used.</param>
    /// <param name="sqlRequestOptions">Execution options such as buffering, timeout, and command type.</param>
    /// <param name="executorFactory">
    /// A higher‑order function that accepts a <see cref="DbTransaction"/> and returns a delegate
    /// that executes a <see cref="CommandDefinition"/> and returns a <see cref="Task{TResult}"/>.
    /// </param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution result.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="commandText"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="dbTransaction"/> or <paramref name="sqlRequestOptions"/> is null.
    /// </exception>
    public async Task<TResult> ExecuteAsync<TResult>(
        string commandText,
        DbTransaction dbTransaction,
        SqlRequestOptions sqlRequestOptions,
        Func<DbTransaction, Func<CommandDefinition, Task<TResult>>> executorFactory,
        CancellationToken cancellationToken = default)
    {
        Validate(commandText, dbTransaction);

        CommandDefinition command =
            commandDefinitionFactory.CreateCommandDefinition(
                commandText,
                dbTransaction,
                sqlRequestOptions,
                cancellationToken
            );

        // Always use the connection associated with the transaction
        return await executorFactory(dbTransaction)(command);
    }

    /// <summary>
    /// Validates required inputs before constructing and executing a Dapper command.
    /// </summary>
    /// <param name="queryText">The SQL query text to validate.</param>
    /// <param name="transaction">The database transaction whose connection will be used.</param>
    /// <param name="options">The SQL execution options to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="queryText"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="transaction"/> or <paramref name="options"/> is null.
    /// </exception>
    private static void Validate(string queryText, DbTransaction transaction)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            throw new ArgumentException(
                "Sql command text must not be null or empty.", nameof(queryText));
        }

        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(transaction.Connection);
    }
}
