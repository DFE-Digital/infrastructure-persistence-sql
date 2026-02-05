using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Executor;
using Moq;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.TestDoubles;

internal static class SqlExecutionOrchestratorTestDouble
{
    public static Mock<ISqlExecutionOrchestrator> ExecutionOrchestratorMock() => new();

    public static Mock<ISqlExecutionOrchestrator> CreateExecutionOrchestratorMockFor<TReturnType>(
        TReturnType returnValue,
        string sqlCommand,
        DbTransaction transaction,
        SqlRequestOptions options)
    {
        Mock<ISqlExecutionOrchestrator> executionOrchestratorMock = ExecutionOrchestratorMock();
        CancellationToken cancellationToken = CancellationToken.None;

        executionOrchestratorMock
            .Setup(executionOrchestrator =>
                executionOrchestrator.ExecuteAsync(
                    sqlCommand,
                    transaction,
                    options,
                    It.IsAny<Func<DbTransaction, CommandDefinition, Task<TReturnType>>>(),
                    cancellationToken))
                .ReturnsAsync(returnValue)
                .Verifiable();

        return executionOrchestratorMock;
    }

    public static Mock<ISqlExecutionOrchestrator> CreateExecutionOrchestratorMockForX<TReturnType>(TReturnType returnValue, string sqlCommand, DbTransaction transaction, SqlRequestOptions options) { var mock = new Mock<ISqlExecutionOrchestrator>(); mock.Setup(exec => exec.ExecuteAsync<TReturnType>(It.Is<string>(s => s == sqlCommand), It.Is<DbTransaction>(tx => tx == transaction), It.Is<SqlRequestOptions>(opts => opts == options), It.IsAny<Func<DbTransaction, CommandDefinition, Task<TReturnType>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(returnValue).Verifiable(); return mock; }
}