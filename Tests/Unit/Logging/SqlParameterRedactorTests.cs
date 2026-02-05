using Dapper;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging;
using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging.Options;
using System.Collections;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Tests.Logging;

public class SqlParameterRedactorTests
{
    private SqlParameterRedactor CreateRedactor(
        string[] sensitive = null!,
        bool partial = false,
        string placeholder = "***")
    {
        SqlLoggingOptions options = new()
        {
            RedactParameters = sensitive != null ? [.. sensitive] : [],
            EnablePartialRedaction = partial,
            RedactionPlaceholder = placeholder
        };

        return new SqlParameterRedactor(
            Microsoft.Extensions.Options.Options.Create(options));
    }

    [Fact]
    public void Constructor_Throws_WhenOptionsIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new SqlParameterRedactor(null!));
    }

    [Fact]
    public void Redact_ReturnsEmptyObject_WhenParametersNull()
    {
        // Arrange
        SqlParameterRedactor redactor = CreateRedactor();

        // Act
        object? result = redactor.Redact(null!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.GetType().GetProperties());
    }

    [Fact]
    public void Redact_RedactsSensitiveProperties_OnObject()
    {
        // Arrange
        SqlParameterRedactor redactor =
            CreateRedactor(["Password"]);

        var input = new { Username = "bob", Password = "secret" };

        // Act
        Dictionary<string, object?> result =
            (Dictionary<string, object?>)redactor.Redact(input);

        // Assert
        Assert.Equal("bob", result["Username"]);
        Assert.Equal("***", result["Password"]);
    }

    [Fact]
    public void Redact_PartiallyRedacts_WhenEnabled()
    {
        // Arrange
        SqlParameterRedactor redactor =
            CreateRedactor(
                sensitive: ["Token"],
                partial: true,
                placeholder: "###");

        var input = new { Token = "ABCDEFG" };

        // Act
        Dictionary<string, object?> result =
            (Dictionary<string, object?>)redactor.Redact(input);

        // Assert
        Assert.Equal("ABC###", result["Token"]);
    }

    [Fact]
    public void Redact_PartiallyRedacts_ShortStrings()
    {
        // Arrange
        SqlParameterRedactor redactor =
            CreateRedactor(
                sensitive: ["Key"],
                partial: true,
                placeholder: "###");

        var input = new { Key = "AB" };

        // Act
        Dictionary<string, object?> result =
            (Dictionary<string, object?>)redactor.Redact(input);

        // Assert
        Assert.Equal("###", result["Key"]);
    }

    [Fact]
    public void Redact_RedactsSensitiveKeys_InDictionary()
    {
        // Arrange
        SqlParameterRedactor redactor =
            CreateRedactor(["Secret"]);

        IDictionary dict = new Dictionary<string, object?>
        {
            ["Name"] = "Alice",
            ["Secret"] = "TopSecret"
        };

        // Act
        Dictionary<string, object?> result =
            (Dictionary<string, object?>)redactor.Redact(dict);

        // Assert
        Assert.Equal("Alice", result["Name"]);
        Assert.Equal("***", result["Secret"]);
    }

    [Fact]
    public void Redact_Redacts_DapperDynamicParameters()
    {
        // Arrange
        SqlParameterRedactor redactor =
            CreateRedactor(["Password"]);

        DynamicParameters dp = new();
        dp.Add("User", "bob");
        dp.Add("Password", "mypassword");

        // Act
        Dictionary<string, object?> result =
            (Dictionary<string, object?>)redactor.Redact(dp);

        // Assert
        Assert.Equal("bob", result["User"]);
        Assert.Equal("***", result["Password"]);
    }

    [Fact]
    public void Redact_IgnoresDynamicParameters_WhenReflectionFails()
    {
        // Arrange
        SqlParameterRedactor redactor =
            CreateRedactor(["X"]);

        var fake = new { ParameterNames = new[] { "A" } };

        // Act
        Dictionary<string, object?> result =
            (Dictionary<string, object?>)redactor.Redact(fake);

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey("ParameterNames"));
        Assert.Equal(new[] { "A" }, result["ParameterNames"]);
    }


    [Fact]
    public void Redact_IsCaseInsensitive()
    {
        // Arrange
        SqlParameterRedactor redactor =
            CreateRedactor(["token"]);

        var input = new { ToKeN = "VALUE" };

        // Act
        Dictionary<string, object?> result =
            (Dictionary<string, object?>)redactor.Redact(input);

        // Assert
        Assert.Equal("***", result["ToKeN"]);
    }

    [Fact]
    public void Redact_LeavesNonSensitiveValues()
    {
        // Arrange
        SqlParameterRedactor redactor =
            CreateRedactor(["Secret"]);

        var input = new { Name = "Alice" };

        // Act
        Dictionary<string, object?> result =
            (Dictionary<string, object?>)redactor.Redact(input);

        // Assert
        Assert.Equal("Alice", result["Name"]);
    }
}

