namespace Inventorization.Base.Abstractions;

/// <summary>
/// Metadata for a single property/column in a domain entity.
/// Provides information for validation, EF configuration, and UI generation.
/// </summary>
public interface IDataPropertyMetadata
{
    /// <summary>
    /// Property name in the C# entity class
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Display name for UI/documentation
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Property CLR type
    /// </summary>
    Type PropertyType { get; }

    /// <summary>
    /// Database column name (if different from property name)
    /// </summary>
    string? ColumnName { get; }

    /// <summary>
    /// Database column type (e.g., "varchar(100)", "decimal(18,2)")
    /// </summary>
    string? ColumnType { get; }

    /// <summary>
    /// Is this property required (NOT NULL in database)
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    /// Maximum length for string properties
    /// </summary>
    int? MaxLength { get; }

    /// <summary>
    /// Minimum value for numeric properties
    /// </summary>
    object? MinValue { get; }

    /// <summary>
    /// Maximum value for numeric properties
    /// </summary>
    object? MaxValue { get; }

    /// <summary>
    /// Precision for decimal properties (total number of digits)
    /// </summary>
    int? Precision { get; }

    /// <summary>
    /// Scale for decimal properties (digits after decimal point)
    /// </summary>
    int? Scale { get; }

    /// <summary>
    /// Is this property the primary key
    /// </summary>
    bool IsPrimaryKey { get; }

    /// <summary>
    /// Is this property a foreign key
    /// </summary>
    bool IsForeignKey { get; }

    /// <summary>
    /// Is this property unique
    /// </summary>
    bool IsUnique { get; }

    /// <summary>
    /// Is this property indexed
    /// </summary>
    bool IsIndexed { get; }

    /// <summary>
    /// Is this property computed (ignored for inserts/updates)
    /// </summary>
    bool IsComputed { get; }

    /// <summary>
    /// Default value for the property
    /// </summary>
    object? DefaultValue { get; }

    /// <summary>
    /// SQL default value expression
    /// </summary>
    string? DefaultValueSql { get; }

    /// <summary>
    /// Regular expression pattern for string validation
    /// </summary>
    string? RegexPattern { get; }

    /// <summary>
    /// Custom validation error message
    /// </summary>
    string? ValidationMessage { get; }

    /// <summary>
    /// Description for documentation
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Custom metadata key-value pairs
    /// </summary>
    IReadOnlyDictionary<string, object>? CustomMetadata { get; }
}
