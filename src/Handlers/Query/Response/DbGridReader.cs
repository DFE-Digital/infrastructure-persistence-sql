using Dapper;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query.Response;

/// <summary>
/// Wraps a Dapper <see cref="SqlMapper.GridReader"/> and exposes a safe,
/// strongly‑typed interface for consuming multiple result sets returned
/// from a single SQL query.
/// </summary>
public class DatabaseGridReader(SqlMapper.GridReader? gridReader) : IDbGridReader
{
    /// <summary>
    /// Finalizer to ensure unmanaged resources are released if the reader
    /// was not disposed explicitly.
    /// </summary>
    ~DatabaseGridReader() => Dispose(false);

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the underlying <see cref="SqlMapper.GridReader"/> instance.
    /// </summary>
    /// <param name="disposing">
    /// Indicates whether the method is being called explicitly (true) or
    /// from the finalizer (false).
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            gridReader?.Dispose();
        }

        gridReader = null;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs asynchronous disposal of the underlying grid reader.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (gridReader is not null)
        {
            await gridReader.DisposeAsync().ConfigureAwait(false);
        }

        gridReader = null;
    }

    /// <summary>
    /// Reads the next result set and maps all rows to the specified type.
    /// </summary>
    /// <typeparam name="TRowType">The type to map each row to.</typeparam>
    /// <returns>A sequence of mapped results.</returns>
    public Task<IEnumerable<TRowType>> ReadAsync<TRowType>()
    {
        EnsureNotDisposed();
        return gridReader!.ReadAsync<TRowType>();
    }

    /// <summary>
    /// Reads the next result set synchronously and maps all rows to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to map each row to.</typeparam>
    public IEnumerable<TRowType> Read<TRowType>()
    {
        EnsureNotDisposed();
        return gridReader!.Read<TRowType>();
    }

    /// <summary>
    /// Reads the next result set and returns the first row.
    /// Throws if no rows are returned.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    public Task<TRowType> ReadFirstAsync<TRowType>()
    {
        EnsureNotDisposed();
        return gridReader!.ReadFirstAsync<TRowType>();
    }

    /// <summary>
    /// Reads the next result set synchronously and returns the first row.
    /// Throws if no rows are returned.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    public TResult ReadFirst<TResult>()
    {
        EnsureNotDisposed();
        return gridReader!.ReadFirst<TResult>();
    }

    /// <summary>
    /// Reads the next result set and returns the first row, or null if no rows exist.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    public Task<TResult?> ReadFirstOrDefaultAsync<TResult>()
    {
        EnsureNotDisposed();
        return gridReader!.ReadFirstOrDefaultAsync<TResult>();
    }

    /// <summary>
    /// Reads the next result set synchronously and returns the first row,
    /// or null if no rows exist.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    public TResult? ReadFirstOrDefault<TResult>()
    {
        EnsureNotDisposed();
        return gridReader!.ReadFirstOrDefault<TResult>();
    }

    /// <summary>
    /// Reads the next result set and returns exactly one row.
    /// Throws if zero or more than one row is returned.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    public Task<TResult> ReadSingleAsync<TResult>()
    {
        EnsureNotDisposed();
        return gridReader!.ReadSingleAsync<TResult>();
    }

    /// <summary>
    /// Reads the next result set synchronously and returns exactly one row.
    /// Throws if zero or more than one row is returned.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    public TResult ReadSingle<TResult>()
    {
        EnsureNotDisposed();
        return gridReader!.ReadSingle<TResult>();
    }

    /// <summary>
    /// Reads the next result set and returns a single row or null.
    /// Throws if more than one row is returned.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    public Task<TResult?> ReadSingleOrDefaultAsync<TResult>()
    {
        EnsureNotDisposed();
        return gridReader!.ReadSingleOrDefaultAsync<TResult>();
    }

    /// <summary>
    /// Reads the next result set synchronously and returns a single row or null.
    /// Throws if more than one row is returned.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    public TResult? ReadSingleOrDefault<TResult>()
    {
        EnsureNotDisposed();
        return gridReader!.ReadSingleOrDefault<TResult>();
    }

    /// <summary>
    /// Ensures the underlying grid reader has not been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when attempting to read after disposal.
    /// </exception>
    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(gridReader is null, nameof(DatabaseGridReader));
    }
}
