using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Command;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Options;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Connection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Context;

/// <summary>
/// Provides scoped access to a database connection and transaction context for Dapper-based operations.
/// Manages connection lifecycle, transaction boundaries, and delegates query/command execution.
/// </summary>
/// <typeparam name="TName">The database identifier type used to resolve connection configuration.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="DbContextProvider{TName}"/> class.
/// </remarks>
/// <param name="sqlQueryHandler">The query handler for executing SQL queries.</param>
/// <param name="sqlCommandHandler">The command handler for executing non-query SQL commands.</param>
/// <param name="dbConnectionFactory">Factory for creating database connections.</param>
/// <param name="dbOptions">Database configuration options.</param>
/// <param name="logger">Logger for diagnostic output.</param>
public sealed class DbContextProvider<TName>(
    ISqlQueryHandler sqlQueryHandler,
    ISqlCommandHandler sqlCommandHandler,
    DbConnectionFactory<TName> dbConnectionFactory,
    IOptions<DatabaseOptions> dbOptions,
    ILogger<DbContextProvider<TName>> logger) :
        IDbContextProvider, IAsyncDisposable, IDisposable where TName : IDbName
{
    private bool _disposed;
    private DbConnection? _connection;
    private DbTransaction? _transaction;
    private readonly DatabaseOptions _options = dbOptions?.Value ??
        throw new ArgumentNullException(nameof(dbOptions));
    private readonly ILogger<DbContextProvider<TName>> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets the query handler used for executing SQL queries.
    /// </summary>
    public ISqlQueryHandler SqlQueryHandler { get; } = sqlQueryHandler;

    /// <summary>
    /// Gets the command handler used for executing non-query SQL commands.
    /// </summary>
    public ISqlCommandHandler SqlCommandHandler { get; } = sqlCommandHandler;

    /// <summary>
    /// Begins a new database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The initiated <see cref="DbTransaction"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a transaction is already in progress.</exception>
    public async Task<DbTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        EnsureNotDisposed();

        if (_transaction is not null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        DbConnection? connection =
            await GetDbConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

        if (isolationLevel == default)
        {
            isolationLevel = _options.DefaultIsolationLevel;
        }

        _logger.LogDebug("Starting new transaction with isolation level: {IsolationLevel}", isolationLevel);

        _transaction =
            await connection
                .BeginTransactionAsync(
                    isolationLevel, cancellationToken)
                .ConfigureAwait(false);

        return _transaction;
    }

    /// <summary>
    /// Ensures the database connection is open and ready for use.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task ConnectAsync(CancellationToken cancellationToken) =>
        await GetDbConnectionAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Gets the current open database connection, opening it if necessary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An open <see cref="DbConnection"/> instance.</returns>
    private async ValueTask<DbConnection> GetDbConnectionAsync(CancellationToken cancellationToken)
    {
        EnsureNotDisposed();

        if (_connection is null)
        {
            _logger.LogDebug("Creating new database connection...");
            _connection = dbConnectionFactory();
        }

        if (_connection.State != ConnectionState.Open)
        {
            _logger.LogDebug("Opening database connection...");
            await _connection.OpenAsync(cancellationToken)
                .ConfigureAwait(false);

            if (_options.OnConnectionOpen is not null)
            {
                await _options.OnConnectionOpen(_connection, cancellationToken)
                    .ConfigureAwait(false);
            }

            _logger.LogInformation("Database connection opened successfully.");
        }

        return _connection;
    }

    /// <summary>
    /// Throws if the context has already been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the context is disposed.</exception>
    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(DbContextProvider<TName>));
    }

    /// <summary>
    /// Asynchronously releases unmanaged and managed resources used by the current instance.
    /// </summary>
    /// <remarks>
    /// This method calls <see cref="DisposeAsyncCore"/> to release resources asynchronously,
    /// then invokes <see cref="Dispose(bool)"/> with <c>false</c> to finalize cleanup.
    /// It also suppresses finalization to prevent the finalizer from running.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Synchronously releases unmanaged and managed resources used by the current instance.
    /// </summary>
    /// <remarks>
    /// This method invokes <see cref="Dispose(bool)"/> with <c>true</c> to release resources,
    /// and suppresses finalization to prevent the finalizer from running.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure unmanaged resources are released.
    /// </summary>
    ~DbContextProvider() => Dispose(false);

    /// <summary>
    /// Disposes managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">Whether the method was called from <see cref="Dispose"/>.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed) { return; }

        if (disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }

        _transaction = null;
        _connection = null;
        _disposed = true;
    }

    /// <summary>
    /// Asynchronously disposes managed resources.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    private async ValueTask DisposeAsyncCore()
    {
        if (_disposed) return;

        if (_transaction is not null)
        {
            await _transaction.DisposeAsync()
                .ConfigureAwait(false);
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync()
                .ConfigureAwait(false);
        }

        _transaction = null;
        _connection = null;
        _disposed = true;
    }
}