using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;

/// <summary>
/// Provides a reusable execution pipeline for SQL commands and queries.
/// This class centralizes logging, parameter redaction, cancellation safety,
/// timing, and exception handling. Callers supply a delegate that determines
/// the actual Dapper operation to perform.
/// </summary>
public sealed class SqlExecutionOrchestrator : ISqlExecutionOrchestrator
{
    private readonly ISqlCommandExecutor _commandExecutor;
    private readonly ISqlParameterRedactor _redactor;
    private readonly ILogger<SqlExecutionOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlExecutionOrchestrator"/> class.
    /// </summary>
    /// <param name="commandExecutor">
    /// The component responsible for constructing <see cref="CommandDefinition"/> instances
    /// and invoking the underlying Dapper execution.
    /// </param>
    /// <param name="redactor">
    /// A service that redacts sensitive SQL parameters before they are logged.
    /// </param>
    /// <param name="logger">The logger used for structured SQL execution logging.</param>
    public SqlExecutionOrchestrator(
        ISqlCommandExecutor commandExecutor,
        ISqlParameterRedactor redactor,
        ILogger<SqlExecutionOrchestrator> logger)
    {
        _commandExecutor = commandExecutor ?? throw new ArgumentNullException(nameof(commandExecutor));
        _redactor = redactor ?? throw new ArgumentNullException(nameof(redactor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a SQL command or query using a caller‑provided delegate that determines
    /// the actual Dapper operation to perform.
    /// </summary>
    /// <typeparam name="TResult">The type returned by the Dapper operation.</typeparam>
    /// <param name="sql">The SQL text to execute.</param>
    /// <param name="transaction">The active transaction whose connection will be used.</param>
    /// <param name="options">Execution options including parameters, timeout, and buffering.</param>
    /// <param name="executor">
    /// A delegate that receives the <see cref="DbTransaction"/> and constructed
    /// <see cref="CommandDefinition"/>, and performs the actual Dapper call.
    /// </param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The result of the Dapper operation.</returns>
    public async Task<TResult> ExecuteAsync<TResult>(
        string sql,
        DbTransaction transaction,
        SqlRequestOptions options,
        Func<DbTransaction, CommandDefinition, Task<TResult>> executor,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(transaction.Connection);
        ArgumentNullException.ThrowIfNull(executor);

        cancellationToken.ThrowIfCancellationRequested();

        object safeParams = _redactor.Redact(options.Parameters ?? new object());

        _logger.LogDebug(
            "Executing SQL. Sql: {Sql}, Parameters: {@Params}", sql, safeParams);

        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            TResult result =
                await _commandExecutor.ExecuteAsync<TResult>(
                    sql,
                    transaction,
                    options,
                    tx => cmd => executor(tx, cmd),
                    cancellationToken
                );

            stopwatch.Stop();

            _logger.LogInformation(
                "SQL executed successfully in {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                "SQL execution cancelled after {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex, "SQL execution failed after {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
