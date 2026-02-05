using System.Data;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers;

/// <summary>
/// Defines execution options for SQL requests, including parameters,
/// command timeout, and command type.
/// </summary>
public readonly record struct SqlRequestOptions
{
    /// <summary>
    /// Represents an empty set of options.
    /// </summary>
    public static SqlRequestOptions None { get; } = new();

    /// <summary>
    /// The SQL parameters to pass to the command.
    /// </summary>
    public object? Parameters { get; init; }

    /// <summary>
    /// The command timeout in seconds. If null, the default timeout is used.
    /// </summary>
    public int? Timeout { get; init; }

    /// <summary>
    /// The type of SQL command being executed (e.g., Text, StoredProcedure).
    /// </summary>
    public CommandType? Type { get; init; }
}