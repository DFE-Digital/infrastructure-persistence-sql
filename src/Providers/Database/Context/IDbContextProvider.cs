using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Command;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query;
using System.Data;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Context;

/// <summary>
/// Defines a contract for managing database connection and transaction lifecycle in Dapper-based persistence.
/// </summary>
public interface IDbContextProvider
{
    /// <summary>
    /// Gets the query handler used for executing SQL queries.
    /// </summary>
    ISqlQueryHandler SqlQueryHandler { get; }

    /// <summary>
    /// Gets the command handler used for executing non-query SQL commands.
    /// </summary>
    ISqlCommandHandler SqlCommandHandler { get; }

    /// <summary>
    /// Begins a new asynchronous database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The isolation level to apply to the transaction.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, returning the initiated <see cref="DbTransaction"/>.</returns>
    Task<DbTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ensures the database connection is established and ready for use.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous connection operation.</returns>
    Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously disposes the database context, releasing any active connections or transactions.
    /// </summary>
    /// <returns>A value task representing the asynchronous dispose operation.</returns>
    ValueTask DisposeAsync();
}