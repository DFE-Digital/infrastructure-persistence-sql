using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging;
using Moq;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.TestDoubles;

internal static class SqlParameterRedactorTestDouble
{
    public static Mock<ISqlParameterRedactor> SqlParameterRedactorMock() => new();

    public static Mock<ISqlParameterRedactor> CreateSqlParameterRedactorMockFor(string redaction)
    {
        Mock<ISqlParameterRedactor> sqlParameterRedactorMock = SqlParameterRedactorMock();

        sqlParameterRedactorMock
            .Setup(redactor =>
                redactor.Redact(It.IsAny<object>()))
            .Returns(new { Id = redaction });

        return sqlParameterRedactorMock;
    }
}
