using System.Collections.Generic;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging.Options
{
    /// <summary>
    /// Represents configuration options that control how SQL parameters are logged
    /// and redacted within the data access layer. These settings allow sensitive
    /// information to be masked before being written to logs, ensuring compliance
    /// with security and privacy requirements.
    ///
    /// This configuration supports:
    /// <list type="bullet">
    ///   <item><description>Full redaction using a placeholder value.</description></item>
    ///   <item><description>Partial redaction for improved traceability.</description></item>
    ///   <item><description>Customizable lists of sensitive parameter names.</description></item>
    /// </list>
    /// </summary>
    public sealed class SqlLoggingOptions
    {
        /// <summary>
        /// A list of parameter name fragments that should be treated as sensitive
        /// and therefore redacted when logged. Matching is case-insensitive and
        /// based on substring containment.
        ///
        /// Examples:
        /// <code>
        /// ["password", "token", "secret", "apikey"]
        /// </code>
        ///
        /// Any parameter whose name contains one of these fragments will be redacted.
        /// </summary>
        public List<string> RedactParameters { get; set; } = new List<string>();

        /// <summary>
        /// The placeholder value used when redacting sensitive parameter values.
        /// This value replaces the original parameter content in logs.
        ///
        /// Example:
        /// <code>
        /// "***REDACTED***"
        /// </code>
        ///
        /// This placeholder is used for both full and partial redaction modes.
        /// </summary>
        public string RedactionPlaceholder { get; set; } = "***REDACTED***";

        /// <summary>
        /// Enables partial redaction of sensitive values. When enabled, the redactor
        /// may preserve a small portion of the original value (e.g., the first few
        /// characters) before appending the redaction placeholder.
        ///
        /// This can be useful for debugging or correlation while still protecting
        /// sensitive data. When disabled, full redaction is applied.
        ///
        /// Example output when enabled:
        /// <code>
        /// "abc***REDACTED***"
        /// </code>
        /// </summary>
        public bool EnablePartialRedaction { get; set; } = false;
    }
}
