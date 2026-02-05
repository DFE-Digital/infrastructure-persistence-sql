namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging;

/// <summary>
/// Defines a service responsible for redacting sensitive SQL parameter values
/// before they are written to logs. Implementations may support multiple
/// parameter types including POCOs, anonymous objects, dictionaries, and
/// Dapper DynamicParameters.
/// </summary>
public interface ISqlParameterRedactor
{
    /// <summary>
    /// Produces a redacted version of the provided parameter object.
    /// Sensitive values are replaced according to configured redaction rules.
    /// </summary>
    /// <param name="parameters">
    /// The original parameter object passed to a SQL command. This may be
    /// a POCO, anonymous object, dictionary, or a Dapper DynamicParameters instance.
    /// </param>
    /// <returns>
    /// A new object containing redacted parameter values, safe for structured logging.
    /// </returns>
    object Redact(object parameters);
}