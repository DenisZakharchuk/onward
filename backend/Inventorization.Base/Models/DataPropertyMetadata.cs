using System.Collections.Frozen;
using Inventorization.Base.Abstractions;

namespace Inventorization.Base.Models;

/// <summary>
/// Concrete implementation of property metadata for domain entities.
/// Provides fluent builder pattern for ease of construction.
/// </summary>
public class DataPropertyMetadata : IDataPropertyMetadata
{
    public string PropertyName { get; init; }
    public string DisplayName { get; init; }
    public Type PropertyType { get; init; }
    public string? ColumnName { get; init; }
    public string? ColumnType { get; init; }
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public object? MinValue { get; init; }
    public object? MaxValue { get; init; }
    public int? Precision { get; init; }
    public int? Scale { get; init; }
    public bool IsPrimaryKey { get; init; }
    public bool IsForeignKey { get; init; }
    public bool IsUnique { get; init; }
    public bool IsIndexed { get; init; }
    public bool IsComputed { get; init; }
    public object? DefaultValue { get; init; }
    public string? DefaultValueSql { get; init; }
    public string? RegexPattern { get; init; }
    public string? ValidationMessage { get; init; }
    public string? Description { get; init; }
    public IReadOnlyDictionary<string, object>? CustomMetadata { get; init; }

    /// <summary>
    /// Minimal constructor for required fields
    /// </summary>
    public DataPropertyMetadata(string propertyName, Type propertyType)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
        DisplayName = propertyName; // Default to property name
    }

    /// <summary>
    /// Full constructor for maximum control
    /// </summary>
    public DataPropertyMetadata(
        string propertyName,
        Type propertyType,
        string? displayName = null,
        string? columnName = null,
        string? columnType = null,
        bool isRequired = false,
        int? maxLength = null,
        object? minValue = null,
        object? maxValue = null,
        int? precision = null,
        int? scale = null,
        bool isPrimaryKey = false,
        bool isForeignKey = false,
        bool isUnique = false,
        bool isIndexed = false,
        bool isComputed = false,
        object? defaultValue = null,
        string? defaultValueSql = null,
        string? regexPattern = null,
        string? validationMessage = null,
        string? description = null,
        IReadOnlyDictionary<string, object>? customMetadata = null)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
        DisplayName = displayName ?? propertyName;
        ColumnName = columnName;
        ColumnType = columnType;
        IsRequired = isRequired;
        MaxLength = maxLength;
        MinValue = minValue;
        MaxValue = maxValue;
        Precision = precision;
        Scale = scale;
        IsPrimaryKey = isPrimaryKey;
        IsForeignKey = isForeignKey;
        IsUnique = isUnique;
        IsIndexed = isIndexed;
        IsComputed = isComputed;
        DefaultValue = defaultValue;
        DefaultValueSql = defaultValueSql;
        RegexPattern = regexPattern;
        ValidationMessage = validationMessage;
        Description = description;
        CustomMetadata = customMetadata;
    }
}

/// <summary>
/// Fluent builder for DataPropertyMetadata
/// </summary>
public class DataPropertyMetadataBuilder
{
    private string _propertyName = string.Empty;
    private Type _propertyType = typeof(object);
    private string? _displayName;
    private string? _columnName;
    private string? _columnType;
    private bool _isRequired;
    private int? _maxLength;
    private object? _minValue;
    private object? _maxValue;
    private int? _precision;
    private int? _scale;
    private bool _isPrimaryKey;
    private bool _isForeignKey;
    private bool _isUnique;
    private bool _isIndexed;
    private bool _isComputed;
    private object? _defaultValue;
    private string? _defaultValueSql;
    private string? _regexPattern;
    private string? _validationMessage;
    private string? _description;
    private Dictionary<string, object>? _customMetadata;

    public DataPropertyMetadataBuilder WithProperty(string propertyName, Type propertyType)
    {
        _propertyName = propertyName;
        _propertyType = propertyType;
        return this;
    }

    public DataPropertyMetadataBuilder WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    public DataPropertyMetadataBuilder WithColumn(string columnName, string? columnType = null)
    {
        _columnName = columnName;
        _columnType = columnType;
        return this;
    }

    public DataPropertyMetadataBuilder Required(string? validationMessage = null)
    {
        _isRequired = true;
        _validationMessage = validationMessage;
        return this;
    }

    public DataPropertyMetadataBuilder WithMaxLength(int maxLength)
    {
        _maxLength = maxLength;
        return this;
    }

    public DataPropertyMetadataBuilder WithRange(object? minValue, object? maxValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        return this;
    }

    public DataPropertyMetadataBuilder WithDecimal(int precision, int scale)
    {
        _precision = precision;
        _scale = scale;
        return this;
    }

    public DataPropertyMetadataBuilder AsPrimaryKey()
    {
        _isPrimaryKey = true;
        _isRequired = true;
        return this;
    }

    public DataPropertyMetadataBuilder AsForeignKey()
    {
        _isForeignKey = true;
        return this;
    }

    public DataPropertyMetadataBuilder AsUnique()
    {
        _isUnique = true;
        return this;
    }

    public DataPropertyMetadataBuilder AsIndexed()
    {
        _isIndexed = true;
        return this;
    }

    public DataPropertyMetadataBuilder AsComputed()
    {
        _isComputed = true;
        return this;
    }

    public DataPropertyMetadataBuilder WithDefault(object? defaultValue)
    {
        _defaultValue = defaultValue;
        return this;
    }

    public DataPropertyMetadataBuilder WithDefaultSql(string defaultValueSql)
    {
        _defaultValueSql = defaultValueSql;
        return this;
    }

    public DataPropertyMetadataBuilder WithRegex(string regexPattern)
    {
        _regexPattern = regexPattern;
        return this;
    }

    public DataPropertyMetadataBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public DataPropertyMetadataBuilder WithCustomMetadata(string key, object value)
    {
        _customMetadata ??= new Dictionary<string, object>();
        _customMetadata[key] = value;
        return this;
    }

    public DataPropertyMetadata Build()
    {
        return new DataPropertyMetadata(
            _propertyName,
            _propertyType,
            _displayName,
            _columnName,
            _columnType,
            _isRequired,
            _maxLength,
            _minValue,
            _maxValue,
            _precision,
            _scale,
            _isPrimaryKey,
            _isForeignKey,
            _isUnique,
            _isIndexed,
            _isComputed,
            _defaultValue,
            _defaultValueSql,
            _regexPattern,
            _validationMessage,
            _description,
            _customMetadata?.ToFrozenDictionary());
    }
}
