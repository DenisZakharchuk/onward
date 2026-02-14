using System.Collections;

namespace Inventorization.Base.Models;

/// <summary>
/// Represents a transformation result with a known schema.
/// Extends Dictionary&lt;string, object?&gt; but includes type metadata for each field.
/// 
/// The schema is constrained by:
/// - Request aliases (user-defined output field names)
/// - Bounded context data model (entity field types)
/// - Transformation type inference (from ProjectionField.GetOutputType())
/// 
/// This provides the perfect balance:
/// - Dynamic field names (user aliases in request)
/// - Type safety (runtime validation against inferred types)
/// - Swagger documentation (schema generation from transformation request)
/// - Better than plain Dictionary (we know expected types)
/// </summary>
public class TransformationResult : Dictionary<string, object?>
{
    private readonly Dictionary<string, Type> _schema;

    /// <summary>
    /// Creates a new transformation result with the specified schema
    /// </summary>
    /// <param name="schema">Dictionary mapping field names to their expected types</param>
    public TransformationResult(Dictionary<string, Type> schema) : base(StringComparer.OrdinalIgnoreCase)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
    }

    /// <summary>
    /// Creates a new transformation result with schema and initial values
    /// </summary>
    public TransformationResult(Dictionary<string, Type> schema, Dictionary<string, object?> values) 
        : base(values, StringComparer.OrdinalIgnoreCase)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        ValidateValues();
    }

    /// <summary>
    /// Gets the schema (field name → type mapping) for this transformation result
    /// </summary>
    public IReadOnlyDictionary<string, Type> Schema => _schema;

    /// <summary>
    /// Gets a value with type safety, validating against the schema
    /// </summary>
    /// <typeparam name="T">Expected type</typeparam>
    /// <param name="fieldName">Field name</param>
    /// <returns>Typed value or default if not found</returns>
    /// <exception cref="KeyNotFoundException">Field not in schema</exception>
    /// <exception cref="InvalidCastException">Type mismatch</exception>
    public T? GetTypedValue<T>(string fieldName)
    {
        if (!_schema.TryGetValue(fieldName, out var expectedType))
        {
            throw new KeyNotFoundException(
                $"Field '{fieldName}' is not defined in the transformation schema. " +
                $"Available fields: {string.Join(", ", _schema.Keys)}");
        }

        var requestedType = typeof(T);
        
        // Handle nullable value types
        var underlyingType = Nullable.GetUnderlyingType(expectedType);
        var actualExpectedType = underlyingType ?? expectedType;
        
        var underlyingRequestedType = Nullable.GetUnderlyingType(requestedType);
        var actualRequestedType = underlyingRequestedType ?? requestedType;

        // Check type compatibility
        if (actualRequestedType != actualExpectedType && 
            !actualRequestedType.IsAssignableFrom(actualExpectedType))
        {
            throw new InvalidCastException(
                $"Type mismatch for field '{fieldName}': schema declares {expectedType.Name}, " +
                $"but requested type is {typeof(T).Name}");
        }

        if (!TryGetValue(fieldName, out var value))
        {
            return default;
        }

        if (value == null)
        {
            return default;
        }

        return (T?)value;
    }

    /// <summary>
    /// Tries to get a typed value without throwing exceptions
    /// </summary>
    public bool TryGetTypedValue<T>(string fieldName, out T? value)
    {
        try
        {
            value = GetTypedValue<T>(fieldName);
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Checks if a field exists in the schema
    /// </summary>
    public bool HasField(string fieldName) => _schema.ContainsKey(fieldName);

    /// <summary>
    /// Gets the expected type for a field
    /// </summary>
    public Type? GetFieldType(string fieldName) => 
        _schema.TryGetValue(fieldName, out var type) ? type : null;

    /// <summary>
    /// Validates that all current values match the schema types
    /// </summary>
    private void ValidateValues()
    {
        foreach (var kvp in this)
        {
            if (!_schema.TryGetValue(kvp.Key, out var expectedType))
            {
                throw new InvalidOperationException(
                    $"Value provided for field '{kvp.Key}' which is not in the schema");
            }

            if (kvp.Value != null && !expectedType.IsInstanceOfType(kvp.Value))
            {
                throw new InvalidOperationException(
                    $"Value for field '{kvp.Key}' is {kvp.Value.GetType().Name}, " +
                    $"but schema expects {expectedType.Name}");
            }
        }
    }

    /// <summary>
    /// Adds or sets a value with schema validation
    /// </summary>
    public new object? this[string key]
    {
        get => base[key];
        set
        {
            if (!_schema.TryGetValue(key, out var expectedType))
            {
                throw new InvalidOperationException(
                    $"Cannot set field '{key}': not defined in schema. " +
                    $"Available fields: {string.Join(", ", _schema.Keys)}");
            }

            if (value != null && !expectedType.IsInstanceOfType(value))
            {
                throw new InvalidOperationException(
                    $"Cannot set field '{key}' to {value.GetType().Name}: " +
                    $"schema expects {expectedType.Name}");
            }

            base[key] = value;
        }
    }

    /// <summary>
    /// Gets the schema as OpenAPI-compatible type names for documentation
    /// </summary>
    public Dictionary<string, string> GetOpenApiSchema()
    {
        return _schema.ToDictionary(
            kvp => kvp.Key,
            kvp => GetOpenApiTypeName(kvp.Value));
    }

    private static string GetOpenApiTypeName(Type type)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.Name switch
        {
            nameof(String) => "string",
            nameof(Int32) => "integer",
            nameof(Int64) => "integer",
            nameof(Decimal) => "number",
            nameof(Double) => "number",
            nameof(Single) => "number",
            nameof(Boolean) => "boolean",
            nameof(DateTime) => "string (date-time)",
            nameof(Guid) => "string (uuid)",
            _ => underlyingType.Name
        };
    }
}
