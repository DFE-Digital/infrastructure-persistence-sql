using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Handlers.Factories;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Options;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Handlers.Factories;

public class CommandDefinitionFactoryTests
{
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;

    public CommandDefinitionFactoryTests()
    {
        // Real SQLite in-memory connection (avoids mocking DbTransaction)
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    [Fact]
    public void Constructor_Throws_WhenDbOptionsIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CommandDefinitionFactory(null!));
    }

    [Fact]
    public void CreateCommandDefinition_Throws_WhenParametersIsNull()
    {
        // Arrange
        IOptions<DatabaseOptions> dbOptions =
            Microsoft.Extensions.Options.Options
                .Create(new DatabaseOptions { DefaultTimeout = 30 });

        CommandDefinitionFactory factory = new(dbOptions);

        SqlRequestOptions requestOptions = new()
        {
            Parameters = null // triggers exception
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            factory.CreateCommandDefinition(
                "SELECT 1",
                _transaction,
                requestOptions,
                CancellationToken.None));
    }

    [Fact]
    public void CreateCommandDefinition_UsesDefaultTimeout_WhenRequestTimeoutIsNull()
    {
        // Arrange
        IOptions<DatabaseOptions> dbOptions =
            Microsoft.Extensions.Options.Options
                .Create(new DatabaseOptions { DefaultTimeout = 42 });

        CommandDefinitionFactory factory = new(dbOptions);

        SqlRequestOptions requestOptions =new()
        {
            Parameters = new { Id = 1 },
            Timeout = null
        };

        // Act
        CommandDefinition cmd =
            factory.CreateCommandDefinition(
                "SELECT 1",
                _transaction,
                requestOptions,
                CancellationToken.None);

        // Assert
        Assert.Equal(42, cmd.CommandTimeout);
    }

    [Fact]
    public void CreateCommandDefinition_UsesRequestTimeout_WhenProvided()
    {
        // Arrange
        IOptions<DatabaseOptions> dbOptions =
            Microsoft.Extensions.Options.Options
                .Create(new DatabaseOptions { DefaultTimeout = 42 });

        CommandDefinitionFactory factory = new(dbOptions);

        SqlRequestOptions requestOptions = new()
        {
            Parameters = new { Id = 1 },
            Timeout = 5
        };

        // Act
        CommandDefinition cmd =
            factory.CreateCommandDefinition(
                "SELECT 1",
                _transaction,
                requestOptions,
                CancellationToken.None);

        // Assert
        Assert.Equal(5, cmd.CommandTimeout);
    }

    [Fact]
    public void CreateCommandDefinition_SetsAllFieldsCorrectly()
    {
        // Arrange
        IOptions<DatabaseOptions> dbOptions =
            Microsoft.Extensions.Options.Options
                .Create(new DatabaseOptions { DefaultTimeout = 42 });

        CommandDefinitionFactory factory = new(dbOptions);

        SqlRequestOptions requestOptions = new()
        {
            Parameters = new { Name = "Test" },
            Timeout = 10,
            Type = System.Data.CommandType.StoredProcedure
        };

        CancellationToken token = new CancellationTokenSource().Token;

        // Act
        CommandDefinition cmd =
            factory.CreateCommandDefinition(
                "EXEC TestProc",
                _transaction,
                requestOptions,
                token);

        // Assert
        Assert.Equal("EXEC TestProc", cmd.CommandText);
        Assert.Equal(requestOptions.Parameters, cmd.Parameters);
        Assert.Equal(_transaction, cmd.Transaction);
        Assert.Equal(10, cmd.CommandTimeout);
        Assert.Equal(System.Data.CommandType.StoredProcedure, cmd.CommandType);
        Assert.Equal(token, cmd.CancellationToken);
    }
}
