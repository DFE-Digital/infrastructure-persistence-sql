using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Factories;
using Moq;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.Executor.TestDoubles;

internal static class CommandDefinitionFactoryTestDouble
{
    public static Mock<ICommandDefinitionFactory> CommandDefinitionFactoryMock() => new();

    public static Mock<ICommandDefinitionFactory> CreateCommandDefinitionFactoryMockFor(
        CommandDefinition returnValue,
        string sqlCommand,
        DbTransaction transaction,
        SqlRequestOptions options)
    {
        Mock<ICommandDefinitionFactory> commandDefinitionFactorymock = new();

        commandDefinitionFactorymock.Setup(factory =>
            factory.CreateCommandDefinition(
                It.Is<string>(sql => sql == sqlCommand),
                It.Is<DbTransaction>(tx => tx == transaction),
                It.Is<SqlRequestOptions>(opts => opts == options),
                It.IsAny<CancellationToken>()))
            .Returns(returnValue)
            .Verifiable();

        return commandDefinitionFactorymock;
    }
}
