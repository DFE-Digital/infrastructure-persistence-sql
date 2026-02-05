using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Command;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Query;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Options;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Connection;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Context;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Context.TestDoubles;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Providers.Database.Context;

public class DbContextProviderTests
{
    public sealed class TestDbName : IDbName { }

    private static DbContextProvider<TestDbName> CreateProvider(
        DbConnectionFake? fakeConn = null,
        DatabaseOptions? options = null)
    {
        ISqlQueryHandler queryHandler = Mock.Of<ISqlQueryHandler>();
        ISqlCommandHandler commandHandler = Mock.Of<ISqlCommandHandler>();

        DbConnectionFake conn = fakeConn ?? new DbConnectionFake();
        DbConnectionFactory<TestDbName> factory = () => conn;
        DatabaseOptions dbOptions = options ?? new DatabaseOptions
        {
            DefaultIsolationLevel = IsolationLevel.ReadCommitted
        };

        ILogger<DbContextProvider<TestDbName>> logger =
            Mock.Of<ILogger<DbContextProvider<TestDbName>>>();

        return new DbContextProvider<TestDbName>(
            queryHandler,
            commandHandler,
            factory,
            Microsoft.Extensions.Options.Options.Create(dbOptions),
            logger);
    }

    [Fact]
    public async Task ConnectAsync_OpensConnection_WhenClosed()
    {
        // Arrange
        DbConnectionFake conn = new();
        DbContextProvider<TestDbName> provider = CreateProvider(conn);

        // Act
        await provider.ConnectAsync(default);

        // Assert
        Assert.True(conn.OpenCalled);
        Assert.Equal(ConnectionState.Open, conn.State);
    }

    [Fact]
    public async Task ConnectAsync_Invokes_OnConnectionOpen_WhenConfigured()
    {
        // Arrange
        DbConnectionFake conn = new();
        bool callbackCalled = false;

        DatabaseOptions opts = new()
        {
            OnConnectionOpen = (c, ct) =>
            {
                callbackCalled = true;
                return Task.CompletedTask;
            }
        };

        DbContextProvider<TestDbName> provider = CreateProvider(conn, opts);

        // Act
        await provider.ConnectAsync(default);

        // Assert
        Assert.True(callbackCalled);
    }

    [Fact]
    public async Task BeginTransactionAsync_StartsTransaction_WhenNoneExists()
    {
        // Arrange
        DbConnectionFake conn = new();
        DbContextProvider<TestDbName> provider = CreateProvider(conn);

        // Act
        DbTransaction tx =
            await provider.BeginTransactionAsync(
                IsolationLevel.Serializable, default);

        // Assert
        Assert.NotNull(tx);
        Assert.True(conn.OpenCalled);
    }

    [Fact]
    public async Task BeginTransactionAsync_Uses_DefaultIsolationLevel_WhenNoneProvided()
    {
        // Arrange
        DbConnectionFake conn = new();

        DatabaseOptions opts = new()
        {
            DefaultIsolationLevel = IsolationLevel.Chaos
        };

        DbContextProvider<TestDbName> provider = CreateProvider(conn, opts);

        // Act
        DbTransaction tx =
            await provider.BeginTransactionAsync(default, default);

        // Assert
        Assert.Equal(IsolationLevel.Chaos, tx.IsolationLevel);
    }

    [Fact]
    public async Task BeginTransactionAsync_Throws_WhenTransactionAlreadyExists()
    {
        // Arrange
        DbContextProvider<TestDbName> provider = CreateProvider();

        // Act
        await provider
            .BeginTransactionAsync(
                IsolationLevel.ReadCommitted, default);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await provider
                .BeginTransactionAsync(
                    IsolationLevel.ReadCommitted, default);
        });
    }

    [Fact]
    public async Task DisposeAsync_DisposesConnection_AndTransaction()
    {
        // Arrange
        DbConnectionFake conn = new();
        DbContextProvider<TestDbName> provider = CreateProvider(conn);

        // Act
        await provider.BeginTransactionAsync(IsolationLevel.ReadCommitted, default);
        await provider.DisposeAsync();

        // Assert
        Assert.True(conn.DisposeCalled);
    }

    [Fact]
    public async Task Dispose_DisposesConnection_AndTransaction()
    {
        // Arrange
        DbConnectionFake conn = new();
        DbContextProvider<TestDbName> provider = CreateProvider(conn);

        // Act
        await provider.ConnectAsync(default);
        provider.Dispose();

        // Assert
        Assert.True(conn.DisposeCalled);
    }

    [Fact]
    public async Task Methods_Throw_WhenDisposed()
    {
        // Arrange
        DbContextProvider<TestDbName> provider = CreateProvider();

        // Act
        await provider.DisposeAsync();

        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
        {
            await provider.ConnectAsync(default);
        });
    }
}
