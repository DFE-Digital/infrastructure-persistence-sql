using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query.Response;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.TestDoubles;
using Microsoft.Data.Sqlite;
using Moq;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.Query;

public class SqlQueryHandlerTests
{
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;

    public SqlQueryHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    [Fact]
    public async Task QueryAsync_CallsExecutor_AndReturnsResult()
    {
        // Arrange
        const string sql = "SELECT * FROM Test";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;

        List<string> expected = ["A", "B"];

        Mock<ISqlExecutionOrchestrator> executorMock =
            SqlExecutionOrchestratorTestDouble
                .CreateExecutionOrchestratorMockFor<IEnumerable<string>>(
                    returnValue: expected,
                    sqlCommand: sql,
                    transaction: _transaction,
                    options: options);

        SqlQueryHandler sqlQueryHandler = new(executorMock.Object);

        // Act
        var result =
            await sqlQueryHandler.QueryAsync<string>(
                sql,
                _transaction,
                options,
                cancellationToken);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task QuerySingleAsync_CallsExecutor_AndReturnsResult()
    {
        // Arrange
        const string sql = "SELECT * FROM Test WHERE Id = 1";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;

        Mock<ISqlExecutionOrchestrator> executorMock =
            SqlExecutionOrchestratorTestDouble
                .CreateExecutionOrchestratorMockFor(
                    returnValue: 42,
                    sqlCommand: sql,
                    transaction: _transaction,
                    options: options);

        SqlQueryHandler sqlQueryHandler = new(executorMock.Object);

        // Act
        var result =
            await sqlQueryHandler.QuerySingleAsync<int>(
                sql,
                _transaction,
                options,
                cancellationToken);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_CallsExecutor_AndReturnsResult()
    {
        // Arrange
        const string sql = "SELECT * FROM Test WHERE Id = 999";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;

        Mock<ISqlExecutionOrchestrator> executorMock =
            SqlExecutionOrchestratorTestDouble
                .CreateExecutionOrchestratorMockFor(
                    returnValue: (string?)null,
                    sqlCommand: sql,
                    transaction: _transaction,
                    options: options);

        SqlQueryHandler sqlQueryHandler = new(executorMock.Object);

        var result =
            await sqlQueryHandler.QuerySingleOrDefaultAsync<string>(
                sql,
                _transaction,
                options,
                cancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task QueryFirstAsync_CallsExecutor_AndReturnsResult()
    {
        // Arrange
        const string sql = "SELECT * FROM Test ORDER BY Id";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;

        Mock<ISqlExecutionOrchestrator> executorMock =
            SqlExecutionOrchestratorTestDouble
                .CreateExecutionOrchestratorMockFor(
                    returnValue: "First",
                    sqlCommand: sql,
                    transaction: _transaction,
                    options: options);

        SqlQueryHandler sqlQueryHandler = new(executorMock.Object);

        // Act
        var result =
            await sqlQueryHandler.QueryFirstAsync<string>(
                sql,
                _transaction,
                options,
                cancellationToken);

        // Assert
        Assert.Equal("First", result);
    }

    [Fact]
    public async Task QueryFirstOrDefaultAsync_CallsExecutor_AndReturnsResult()
    {
        // Arrange
        const string sql = "SELECT * FROM Test ORDER BY Id";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;

        Mock<ISqlExecutionOrchestrator> executorMock =
            SqlExecutionOrchestratorTestDouble
                .CreateExecutionOrchestratorMockFor(
                    returnValue: (string?)null,
                    sqlCommand: sql,
                    transaction: _transaction,
                    options: options);

        SqlQueryHandler sqlQueryHandler = new(executorMock.Object);

        // Act
        var result =
            await sqlQueryHandler.QueryFirstOrDefaultAsync<string>(
                sql,
                _transaction,
                options,
                cancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task QueryMultipleAsync_CallsExecutor_AndWrapsGridReader()
    {
        const string sql = "SELECT * FROM Test; SELECT * FROM Other;";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;

        FakeGridReader fakeGridReader = new();

        Mock<ISqlExecutionOrchestrator> executorMock =
            SqlExecutionOrchestratorTestDouble
                .CreateExecutionOrchestratorMockFor(
                    returnValue: fakeGridReader,
                    sqlCommand: sql,
                    transaction: _transaction,
                    options: options);

        SqlQueryHandler sqlQueryHandler = new(executorMock.Object);

        // Act
        var result =
            await sqlQueryHandler.QueryMultipleAsync(
                sql,
                _transaction,
                options,
                cancellationToken);

        // Assert
        Assert.IsType<DatabaseGridReader>(result);
    }

    private sealed class FakeGridReader : SqlMapper.GridReader
    {
        public FakeGridReader() : base(null!, null!, null!) { }
    }
}
