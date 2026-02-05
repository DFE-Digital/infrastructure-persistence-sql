using Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging.Options;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Reflection;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Logging;

/// <summary>
/// Provides reusable logic for redacting sensitive SQL parameters
/// based on configuration-driven rules. Supports POCOs, anonymous objects,
/// dictionaries, and Dapper DynamicParameters.
/// </summary>
public sealed class SqlParameterRedactor : ISqlParameterRedactor
{
    private readonly SqlLoggingOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="SqlParameterRedactor"/>.
    /// </summary>
    /// <param name="options">Configuration options controlling redaction behaviour.</param>
    public SqlParameterRedactor(IOptions<SqlLoggingOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value ??
            throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Produces a redacted version of the provided parameter object.
    /// Supports POCOs, anonymous objects, dictionaries, and DynamicParameters.
    /// </summary>
    /// <param name="parameters">The original parameter object to redact.</param>
    /// <returns>A redacted object safe for logging.</returns>
    public object Redact(object parameters)
    {
        if (parameters is null)
        {
            return new { };
        }

        if (parameters is IDictionary dictionary)
        {
            return RedactDictionary(dictionary);
        }

        if (parameters.GetType().Name == ReflectionNames.DynamicParametersType)
        {
            return RedactDynamicParameters(parameters);
        }

        return RedactObjectProperties(parameters);
    }

    /// <summary>
    /// Redacts values inside a dictionary-like structure.
    /// </summary>
    /// <param name="dictionary">The dictionary containing parameter names and values.</param>
    private Dictionary<string, object?> RedactDictionary(IDictionary dictionary)
    {
        Dictionary<string, object?> result = new Dictionary<string, object?>();

        foreach (DictionaryEntry entry in dictionary)
        {
            // Skip null keys entirely
            if (entry.Key == null)
            {
                continue;
            }

            string key = entry.Key.ToString() ?? string.Empty;
            object? value = entry.Value;

            if (IsSensitive(key))
            {
                result[key] = ApplyRedaction(value);
            }
            else
            {
                result[key] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Handles Dapper DynamicParameters using reflection.
    /// </summary>
    /// <param name="dynamicParams">The DynamicParameters instance to redact.</param>
    private Dictionary<string, object?> RedactDynamicParameters(object dynamicParams)
    {
        Dictionary<string, object?> result = new Dictionary<string, object?>();

        PropertyInfo? namesProp =
            dynamicParams.GetType()
                .GetProperty(ReflectionNames.ParameterNamesProperty);

        if (namesProp is null)
        {
            return result;
        }

        IEnumerable<string> names =
            (IEnumerable<string>)namesProp.GetValue(dynamicParams)!;

        foreach (string name in names)
        {
            object? value = GetDynamicParameterValue(dynamicParams, name);

            if (IsSensitive(name))
            {
                result[name] = ApplyRedaction(value);
            }
            else
            {
                result[name] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Extracts a value from DynamicParameters using reflection.
    /// </summary>
    /// <param name="dynamicParams">The DynamicParameters instance.</param>
    /// <param name="name">The parameter name to retrieve.</param>
    private static object? GetDynamicParameterValue(object dynamicParams, string name)
    {
        try
        {
            FieldInfo? field = dynamicParams.GetType()
                .GetField("parameters", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                return null;
            }

            object? rawDict = field.GetValue(dynamicParams);

            if (rawDict is not IDictionary dictionary || !dictionary.Contains(name))
            {
                return null;
            }

            object? paramInfo = dictionary[name];

            PropertyInfo? valueProp = paramInfo.GetType().GetProperty("Value");
            if (valueProp == null)
            {
                return null;
            }

            return valueProp.GetValue(paramInfo);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Redacts properties on a POCO or anonymous object.
    /// </summary>
    /// <param name="parameters">The object whose properties should be inspected and redacted.</param>
    private Dictionary<string, object?> RedactObjectProperties(object parameters)
    {
        Dictionary<string, object?> result = new Dictionary<string, object?>();

        PropertyInfo[] props = parameters.GetType().GetProperties();

        foreach (PropertyInfo prop in props)
        {
            string name = prop.Name;
            object? value;

            try
            {
                value = prop.GetValue(parameters);
            }
            catch
            {
                value = null;
            }

            if (IsSensitive(name))
            {
                result[name] = ApplyRedaction(value);
            }
            else
            {
                result[name] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether a parameter name should be redacted.
    /// </summary>
    /// <param name="name">The parameter name to evaluate.</param>
    private bool IsSensitive(string name)
    {
        return _options.RedactParameters.Any(
            s => name.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Applies full or partial redaction depending on configuration.
    /// </summary>
    /// <param name="value">The original parameter value.</param>
    private string ApplyRedaction(object? value)
    {
        if (!_options.EnablePartialRedaction)
        {
            return _options.RedactionPlaceholder;
        }

        if (value is string str && str.Length > 3)
        {
            return string.Concat(str.AsSpan(0, 3), _options.RedactionPlaceholder);
        }

        return _options.RedactionPlaceholder;
    }

    /// <summary>
    /// Contains reflection-related constant names used for interacting
    /// with Dapper's DynamicParameters type.
    /// </summary>
    private static class ReflectionNames
    {
        public const string DynamicParametersType = "DynamicParameters";
        public const string ParameterNamesProperty = "ParameterNames";
        public const string GetMethod = "Get";
    }
}
