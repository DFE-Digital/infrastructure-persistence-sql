using System.Data;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Options;

/// <summary>
/// Provides configuration options that influence database connection
/// behaviour and default execution settings.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// An optional callback invoked immediately after a database connection
    /// is opened. Useful for session-level configuration (e.g., SET statements).
    /// </summary>
    /// <remarks>
    /// The delegate receives the open <see cref="DbConnection"/> and the
    /// <see cref="CancellationToken"/> associated with the operation.
    /// </remarks>
    public Func<DbConnection, CancellationToken, Task>? OnConnectionOpen { get; set; }

    /// <summary>
    /// The default command timeout (in seconds) applied when no timeout is
    /// explicitly provided in <see cref="SqlRequestOptions"/>.
    /// </summary>
    public int? DefaultTimeout { get; set; }

    /// <summary>
    /// The default transaction isolation level used when creating new
    /// transactions. Defaults to <see cref="IsolationLevel.ReadCommitted"/>.
    /// </summary>
    public IsolationLevel DefaultIsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
}