using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Connection;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Providers.Database.Connection;

public class DbConnectionFactoryTests
{
    private sealed class TestDbName : IDbName { }

    [Fact]
    public void DbConnectionFactory_Returns_DbConnection()
    {
        // Arrange
        DbConnectionFactory<TestDbName> factory =
            static () => new SqlConnection("Server=.;Database=master;Trusted_Connection=True;");

        // Act
        DbConnection connection = factory();

        // Assert
        Assert.NotNull(connection);
        Assert.IsAssignableFrom<DbConnection>(connection);
        Assert.IsType<SqlConnection>(connection);
    }

    [Fact]
    public void DbConnectionFactory_CanBeInvokedMultipleTimes()
    {
        // Arrange
        DbConnectionFactory<TestDbName> factory =
            static () => new SqlConnection("Server=.;Database=master;Trusted_Connection=True;");

        // Act
        DbConnection conn1 = factory();
        DbConnection conn2 = factory();

        // Assert
        Assert.NotSame(conn1, conn2); // factory should create new instances
    }
}
