using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.Executor.TestDoubles;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.TestDoubles;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.Executor;

public class SqlExecutionOrchestratorTests
{
    private readonly Mock<ILogger<SqlExecutionOrchestrator>> _loggerMock;
    private readonly Mock<ISqlParameterRedactor> _redactorMock;
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;

    public SqlExecutionOrchestratorTests()
    {
        _loggerMock = new Mock<ILogger<SqlExecutionOrchestrator>>();

        _redactorMock =
            SqlParameterRedactorTestDouble
                .CreateSqlParameterRedactorMockFor(redaction: "***");

        // Real SQLite in-memory connection
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResult_AndLogsSuccess()
    {
        // Arrange
        const string sql = "SELECT 1";
        SqlRequestOptions options = new() { Parameters = new { Id = 1 } };
        CancellationToken cancellationToken = CancellationToken.None;

        Mock<ISqlCommandExecutor> commandExecutorMock =
            SqlCommandExecutorTestDouble.CreateSqlCommandExecutorMockFor(
                returnValue: 123,
                sqlCommand: sql,
                transaction: _transaction,
                options);

        SqlExecutionOrchestrator sqlExecutionOrchestrator =
            new(
                commandExecutorMock.Object,
                _redactorMock.Object,
                _loggerMock.Object);

        // Act
        var result =
            await sqlExecutionOrchestrator.ExecuteAsync(
                sql,
                _transaction,
                options,
                (tx, cmd) => Task.FromResult(123),
                cancellationToken);

        // Assert
        Assert.Equal(123, result);

        _loggerMock.VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenSqlIsNull()
    {
        // Arrange
        SqlRequestOptions options = new();

        SqlExecutionOrchestrator sqlExecutionOrchestrator =
            new(
                SqlCommandExecutorTestDouble.SqlCommandExecutorMock().Object,
                _redactorMock.Object,
                _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sqlExecutionOrchestrator.ExecuteAsync<int>(
                null!,
                _transaction,
                options,
                (tx, cmd) => Task.FromResult(0),
                CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenTransactionIsNull()
    {
        // Arrange
        SqlRequestOptions options = new();

        SqlExecutionOrchestrator sqlExecutionOrchestrator =
            new(
                SqlCommandExecutorTestDouble.SqlCommandExecutorMock().Object,
                _redactorMock.Object,
                _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sqlExecutionOrchestrator.ExecuteAsync(
                "SELECT 1",
                null!,
                options,
                (tx, cmd) => Task.FromResult(0),
                CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenExecutorIsNull()
    {
        // Arrange
        SqlRequestOptions options = new();

        SqlExecutionOrchestrator sqlExecutionOrchestrator =
            new(
                SqlCommandExecutorTestDouble.SqlCommandExecutorMock().Object,
                _redactorMock.Object,
                _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sqlExecutionOrchestrator.ExecuteAsync<int>(
                "SELECT 1",
                _transaction,
                options,
                null!,
                CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenCancelled()
    {
        // Arrange
        SqlRequestOptions options = new();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        SqlExecutionOrchestrator sqlExecutionOrchestrator =
            new(
                SqlCommandExecutorTestDouble.SqlCommandExecutorMock().Object,
                _redactorMock.Object,
                _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            sqlExecutionOrchestrator.ExecuteAsync(
                "SELECT 1",
                _transaction,
                options,
                (tx, cmd) => Task.FromResult(0),
                cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_LogsWarning_OnCancellation()
    {
        // Arrange
        const string sql = "SELECT 1";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;

        Mock<ISqlCommandExecutor> executorMock =
            SqlCommandExecutorTestDouble
                .ThrowExceptionFor<int, OperationCanceledException>("Kaboom");

        SqlExecutionOrchestrator sqlExecutionOrchestrator =
            new(
                executorMock.Object,
                _redactorMock.Object,
                _loggerMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            sqlExecutionOrchestrator.ExecuteAsync<int>(
                sql,
                _transaction,
                options,
                (tx, cmd) => throw new OperationCanceledException("Kaboom"),
                cancellationToken));

        // Verify warning log
        _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_LogsError_OnException()
    {
        // Arrange
        const string sql = "SELECT 1";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;

        Mock<ISqlCommandExecutor> executorMock =
            SqlCommandExecutorTestDouble
                .ThrowExceptionFor<int, InvalidOperationException>("Kaboom");

        SqlExecutionOrchestrator sqlExecutionOrchestrator =
            new(
                executorMock.Object,
                _redactorMock.Object,
                _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sqlExecutionOrchestrator.ExecuteAsync<int>(
                sql,
                _transaction,
                options,
                (tx, cmd) => throw new InvalidOperationException("Kaboom"),
                cancellationToken));

        _loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }
}