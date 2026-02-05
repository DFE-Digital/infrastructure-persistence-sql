using Microsoft.Extensions.Logging;
using Moq;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.Executor.TestDoubles;

public static class LoggerMockTestDoubles
{
    public static void VerifyLog(
        this Mock<ILogger> loggerMock,
        LogLevel level,
        Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times);
    }

    public static void VerifyLog<T>(
        this Mock<ILogger<T>> loggerMock,
        LogLevel level,
        Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times);
    }
}

