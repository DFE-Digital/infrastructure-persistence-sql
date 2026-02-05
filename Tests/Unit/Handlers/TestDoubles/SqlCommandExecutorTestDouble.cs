using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using Moq;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.TestDoubles;

internal static class SqlCommandExecutorTestDouble
{
    public static Mock<ISqlCommandExecutor> SqlCommandExecutorMock() => new();

    public static Mock<ISqlCommandExecutor> CreateSqlCommandExecutorMockFor<TReturnType>(
        TReturnType returnValue,
        string sqlCommand,
        DbTransaction transaction,
        SqlRequestOptions options)
    {
        Mock<ISqlCommandExecutor> sqlCommandExecutorMock = SqlCommandExecutorMock();
        CancellationToken cancellationToken = CancellationToken.None;

        sqlCommandExecutorMock
            .Setup(sqlCommandExecutor =>
                sqlCommandExecutor.ExecuteAsync(
                    sqlCommand,
                    transaction,
                    options,
                    It.IsAny<Func<DbTransaction, Func<CommandDefinition, Task<TReturnType>>>>(),
                    cancellationToken))
                .ReturnsAsync(returnValue);

        return sqlCommandExecutorMock;
    }

    public static Mock<ISqlCommandExecutor> ThrowExceptionFor<TResult, TException>(string message)
        where TException : Exception, new()
    {
        Mock<ISqlCommandExecutor> sqlCommandExecutorMock = SqlCommandExecutorMock();

        sqlCommandExecutorMock.Setup(exec =>
            exec.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<DbTransaction>(),
                It.IsAny<SqlRequestOptions>(),
                It.IsAny<Func<DbTransaction, Func<CommandDefinition, Task<TResult>>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync((TException)Activator.CreateInstance(typeof(TException), message)!);

        return sqlCommandExecutorMock;
    }

}