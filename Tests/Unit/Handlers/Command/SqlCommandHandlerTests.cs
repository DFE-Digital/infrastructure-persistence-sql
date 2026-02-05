using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Command;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.TestDoubles;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests;

public class SqlCommandHandlerTests
{
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;

    public SqlCommandHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    [Fact]
    public async Task ExecuteAsync_CallsExecutor_AndReturnsResult()
    {
        // Arrange
        const string sql = "UPDATE Test SET Value = 1";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;
        
        Mock<ISqlExecutionOrchestrator> executorOrchestratorMock =
            SqlExecutionOrchestratorTestDouble
                .CreateExecutionOrchestratorMockFor(
                    returnValue: 42,
                    sqlCommand: sql,
                    transaction: _transaction,
                    options: options);

        SqlCommandHandler handler = new(executorOrchestratorMock.Object);

        // Act
        var result =
            await handler.ExecuteAsync(
                sql, _transaction, options, cancellationToken);

        // Assert/verify
        Assert.Equal(42, result);

        executorOrchestratorMock.Verify(sqlExecutionOrchestrator =>
            sqlExecutionOrchestrator.ExecuteAsync(
                sql,
                _transaction,
                options,
                It.IsAny<Func<DbTransaction, CommandDefinition, Task<int>>>(),
                cancellationToken),
                Times.Once
        );
    }

    [Fact]
    public async Task ExecuteScalarAsync_CallsExecutor_AndReturnsResult()
    {
        // Arrange
        const string sql = "SELECT COUNT(*) FROM Test";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;

        Mock<ISqlExecutionOrchestrator> executorMock =
            SqlExecutionOrchestratorTestDouble
                .CreateExecutionOrchestratorMockFor(
                    returnValue: 123L,
                    sqlCommand: sql,
                    transaction: _transaction,
                    options: options);

        SqlCommandHandler handler = new(executorMock.Object);

        // Act
        var result =
            await handler.ExecuteScalarAsync<long>(
                sql, _transaction, options, cancellationToken);

        // Assert
        Assert.Equal(123L, result);
    }

    [Fact]
    public async Task ExecuteReaderAsync_CallsExecutor_AndReturnsReader()
    {
        // Arrange
        const string sql = "SELECT * FROM Test";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;
        Mock<DbDataReader> readerMock = new();

        Mock<ISqlExecutionOrchestrator> executorMock =
            SqlExecutionOrchestratorTestDouble
                .CreateExecutionOrchestratorMockFor(
                    returnValue: readerMock.Object,
                    sqlCommand: sql,
                    transaction: _transaction,
                    options: options);

        SqlCommandHandler handler = new(executorMock.Object);

        // Act
        var result =
            await handler.ExecuteReaderAsync(
                sql, _transaction, options, cancellationToken);

        // Assert
        Assert.Same(readerMock.Object, result);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesExceptions()
    {
        // Arrange
        Mock<ISqlCommandExecutor> executorMock =
            SqlCommandExecutorTestDouble
                .ThrowExceptionFor<int, InvalidOperationException>("Kaboom");

        Mock<ISqlParameterRedactor> redactor = new();

        ILogger <SqlExecutionOrchestrator> logger =
            new LoggerFactory().CreateLogger<SqlExecutionOrchestrator>();

        SqlExecutionOrchestrator orchestrator =
            new(
                executorMock.Object,
                redactor.Object,
                logger);

        SqlCommandHandler handler = new(orchestrator);

        // Act/Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.ExecuteAsync(
                "DELETE FROM Test", _transaction,
                new SqlRequestOptions(), CancellationToken.None));
    }
}

