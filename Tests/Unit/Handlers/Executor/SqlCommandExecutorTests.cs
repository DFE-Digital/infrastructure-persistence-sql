using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Factories;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.Executor.TestDoubles;
using Microsoft.Data.Sqlite;
using Moq;
using System.Data;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.Executor;

public class SqlCommandExecutorTests
{
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;

    public SqlCommandExecutorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    [Fact]
    public async Task ExecuteAsync_InvokesExecutorFactory_AndReturnsResult()
    {
        // Arrange
        const string sql = "SELECT 1";
        SqlRequestOptions options = new();
        CancellationToken cancellationToken = CancellationToken.None;
        int expectedResult = 123;
        CommandDefinition commandDefinition = new(sql);

        Mock<ICommandDefinitionFactory> commandDefinitionFactoryMock =
            CommandDefinitionFactoryTestDouble
                .CreateCommandDefinitionFactoryMockFor(
                    commandDefinition,
                    sql,
                    _transaction,
                    options);

        Func<DbTransaction, Func<CommandDefinition, Task<int>>> executorFactory =
            dbTransaction => cmd =>
                Task.FromResult(expectedResult);

        SqlCommandExecutor executor = new(commandDefinitionFactoryMock.Object);

        // Act
        var result =
            await executor.ExecuteAsync(
                sql,
                _transaction,
                options,
                executorFactory,
                cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);

        commandDefinitionFactoryMock.Verify(factory =>
            factory.CreateCommandDefinition(
                It.Is<string>(str => str == sql),
                It.IsAny<DbTransaction>(),
                It.IsAny<SqlRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_When_CommandText_IsNullOrWhitespace()
    {
        // Arrange
        SqlRequestOptions options = new ();
        Mock<ICommandDefinitionFactory> commandDefinitionFactoryMock =
            CommandDefinitionFactoryTestDouble.CommandDefinitionFactoryMock();
        SqlCommandExecutor executor = new(commandDefinitionFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            executor.ExecuteAsync<int>(
                " ",
                _transaction,
                options,
                tx => cmd => Task.FromResult(0),
                CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_Throws_When_Transaction_IsNull()
    {
        // Arrange
        SqlRequestOptions options = new();
        Mock<ICommandDefinitionFactory> commandDefinitionFactoryMock =
            CommandDefinitionFactoryTestDouble.CommandDefinitionFactoryMock();
        SqlCommandExecutor executor = new(commandDefinitionFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            executor.ExecuteAsync<int>(
                "SELECT 1",
                null!,
                options,
                tx => cmd => Task.FromResult(0),
                CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_Throws_When_TransactionConnection_IsNull()
    {
        // Arrange
        FakeTransactionWithNullConnection fakeTransaction = new();
        SqlRequestOptions options = new();
        Mock<ICommandDefinitionFactory> commandDefinitionFactoryMock =
            CommandDefinitionFactoryTestDouble.CommandDefinitionFactoryMock();
        SqlCommandExecutor executor = new(commandDefinitionFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            executor.ExecuteAsync<int>(
                "SELECT 1",
                fakeTransaction,
                options,
                tx => cmd => Task.FromResult(0),
                CancellationToken.None));
    }

    private sealed class FakeTransactionWithNullConnection : DbTransaction
    {
        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
        protected override DbConnection DbConnection => null!;
        public override void Commit() { }
        public override void Rollback() { }
    }
}
