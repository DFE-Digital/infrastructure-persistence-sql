namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query.Response;

/// <summary>
/// Represents a strongly‑typed reader for consuming multiple result sets
/// returned from a single SQL query.
/// </summary>
public interface IDbGridReader : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Reads the next result set and maps all rows to the specified type.
    /// </summary>
    /// <typeparam name="TRowType">The type to map each row to.</typeparam>
    /// <returns>A sequence of mapped results.</returns>
    Task<IEnumerable<TRowType>> ReadAsync<TRowType>();

    /// <summary>
    /// Reads the next result set synchronously and maps all rows to the specified type.
    /// </summary>
    /// <typeparam name="TRowType">The type to map each row to.</typeparam>
    IEnumerable<TRowType> Read<TRowType>();

    /// <summary>
    /// Reads the next result set and returns the first row.
    /// Throws if no rows are returned.
    /// </summary>
    /// <typeparam name="TRowType">The expected result type.</typeparam>
    Task<TRowType> ReadFirstAsync<TRowType>();

    /// <summary>
    /// Reads the next result set synchronously and returns the first row.
    /// Throws if no rows are returned.
    /// </summary>
    /// <typeparam name="TRowType">The expected result type.</typeparam>
    TRowType ReadFirst<TRowType>();

    /// <summary>
    /// Reads the next result set and returns the first row, or null if no rows exist.
    /// </summary>
    /// <typeparam name="TRowType">The expected result type.</typeparam>
    Task<TRowType?> ReadFirstOrDefaultAsync<TRowType>();

    /// <summary>
    /// Reads the next result set synchronously and returns the first row,
    /// or null if no rows exist.
    /// </summary>
    /// <typeparam name="TRowType">The expected result type.</typeparam>
    TRowType? ReadFirstOrDefault<TRowType>();

    /// <summary>
    /// Reads the next result set and returns exactly one row.
    /// Throws if zero or more than one row is returned.
    /// </summary>
    /// <typeparam name="TRowType">The expected result type.</typeparam>
    Task<TRowType> ReadSingleAsync<TRowType>();

    /// <summary>
    /// Reads the next result set synchronously and returns exactly one row.
    /// Throws if zero or more than one row is returned.
    /// </summary>
    /// <typeparam name="TRowType">The expected result type.</typeparam>
    TRowType ReadSingle<TRowType>();

    /// <summary>
    /// Reads the next result set and returns a single row or null.
    /// Throws if more than one row is returned.
    /// </summary>
    /// <typeparam name="TRowType">The expected result type.</typeparam>
    Task<TRowType?> ReadSingleOrDefaultAsync<TRowType>();

    /// <summary>
    /// Reads the next result set synchronously and returns a single row or null.
    /// Throws if more than one row is returned.
    /// </summary>
    /// <typeparam name="TRowType">The expected result type.</typeparam>
    TRowType? ReadSingleOrDefault<TRowType>();
}
