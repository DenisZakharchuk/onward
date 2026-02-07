using System.Collections.Frozen;
using Inventorization.Base.Abstractions;

namespace Inventorization.Base.Models;

/// <summary>
/// Concrete implementation of entity metadata for domain models.
/// Provides fluent builder pattern for ease of construction.
/// </summary>
public class DataModelMetadata : IDataModelMetadata
{
    public Type EntityType { get; init; }
    public string EntityName { get; init; }
    public string DisplayName { get; init; }
    public string TableName { get; init; }
    public string? Schema { get; init; }
    public IReadOnlyDictionary<string, IDataPropertyMetadata> Properties { get; init; }
    public IReadOnlyList<string> PrimaryKey { get; init; }
    public IReadOnlyList<string[]> Indexes { get; init; }
    public IReadOnlyList<string[]> UniqueConstraints { get; init; }
    public IReadOnlyList<IRelationshipMetadata> Relationships { get; init; }
    public bool UsesSoftDelete { get; init; }
    public bool IsAuditable { get; init; }
    public string? Description { get; init; }
    public IReadOnlyDictionary<string, object>? CustomMetadata { get; init; }

    /// <summary>
    /// Minimal constructor
    /// </summary>
    public DataModelMetadata(
        Type entityType,
        string tableName,
        IReadOnlyDictionary<string, IDataPropertyMetadata> properties)
    {
        EntityType = entityType;
        EntityName = entityType.Name;
        DisplayName = entityType.Name;
        TableName = tableName;
        Properties = properties is FrozenDictionary<string, IDataPropertyMetadata> 
            ? properties 
            : properties.ToFrozenDictionary();
        PrimaryKey = Array.Empty<string>();
        Indexes = Array.Empty<string[]>();
        UniqueConstraints = Array.Empty<string[]>();
        Relationships = Array.Empty<IRelationshipMetadata>();
    }

    /// <summary>
    /// Full constructor for maximum control
    /// </summary>
    public DataModelMetadata(
        Type entityType,
        string? entityName = null,
        string? displayName = null,
        string? tableName = null,
        string? schema = null,
        IReadOnlyDictionary<string, IDataPropertyMetadata>? properties = null,
        IReadOnlyList<string>? primaryKey = null,
        IReadOnlyList<string[]>? indexes = null,
        IReadOnlyList<string[]>? uniqueConstraints = null,
        IReadOnlyList<IRelationshipMetadata>? relationships = null,
        bool usesSoftDelete = false,
        bool isAuditable = false,
        string? description = null,
        IReadOnlyDictionary<string, object>? customMetadata = null)
    {
        EntityType = entityType;
        EntityName = entityName ?? entityType.Name;
        DisplayName = displayName ?? entityType.Name;
        TableName = tableName ?? entityType.Name + "s";
        Schema = schema;
        Properties = properties is FrozenDictionary<string, IDataPropertyMetadata> 
            ? properties 
            : (properties?.ToFrozenDictionary() ?? FrozenDictionary<string, IDataPropertyMetadata>.Empty);
        PrimaryKey = primaryKey ?? Array.Empty<string>();
        Indexes = indexes ?? Array.Empty<string[]>();
        UniqueConstraints = uniqueConstraints ?? Array.Empty<string[]>();
        Relationships = relationships ?? Array.Empty<IRelationshipMetadata>();
        UsesSoftDelete = usesSoftDelete;
        IsAuditable = isAuditable;
        Description = description;
        CustomMetadata = customMetadata;
    }
}

/// <summary>
/// Generic version for type-safe entity metadata
/// </summary>
public class DataModelMetadata<TEntity> : DataModelMetadata, IDataModelMetadata<TEntity>
    where TEntity : class
{
    public DataModelMetadata(
        string? entityName = null,
        string? displayName = null,
        string? tableName = null,
        string? schema = null,
        IReadOnlyDictionary<string, IDataPropertyMetadata>? properties = null,
        IReadOnlyList<string>? primaryKey = null,
        IReadOnlyList<string[]>? indexes = null,
        IReadOnlyList<string[]>? uniqueConstraints = null,
        IReadOnlyList<IRelationshipMetadata>? relationships = null,
        bool usesSoftDelete = false,
        bool isAuditable = false,
        string? description = null,
        IReadOnlyDictionary<string, object>? customMetadata = null)
        : base(
            entityType: typeof(TEntity),
            entityName: entityName,
            displayName: displayName,
            tableName: tableName,
            schema: schema,
            properties: properties,
            primaryKey: primaryKey,
            indexes: indexes,
            uniqueConstraints: uniqueConstraints,
            relationships: relationships,
            usesSoftDelete: usesSoftDelete,
            isAuditable: isAuditable,
            description: description,
            customMetadata: customMetadata)
    {
    }
}

/// <summary>
/// Fluent builder for DataModelMetadata
/// </summary>
public class DataModelMetadataBuilder<TEntity> where TEntity : class
{
    private string? _entityName;
    private string? _displayName;
    private string? _tableName;
    private string? _schema;
    private readonly Dictionary<string, IDataPropertyMetadata> _properties = new();
    private readonly List<string> _primaryKey = new();
    private readonly List<string[]> _indexes = new();
    private readonly List<string[]> _uniqueConstraints = new();
    private readonly List<IRelationshipMetadata> _relationships = new();
    private bool _usesSoftDelete;
    private bool _isAuditable;
    private string? _description;
    private Dictionary<string, object>? _customMetadata;

    public DataModelMetadataBuilder<TEntity> WithEntityName(string entityName)
    {
        _entityName = entityName;
        return this;
    }

    public DataModelMetadataBuilder<TEntity> WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    public DataModelMetadataBuilder<TEntity> WithTable(string tableName, string? schema = null)
    {
        _tableName = tableName;
        _schema = schema;
        return this;
    }

    public DataModelMetadataBuilder<TEntity> AddProperty(IDataPropertyMetadata property)
    {
        _properties[property.PropertyName] = property;
        return this;
    }

    public DataModelMetadataBuilder<TEntity> AddProperties(params IDataPropertyMetadata[] properties)
    {
        foreach (var property in properties)
        {
            _properties[property.PropertyName] = property;
        }
        return this;
    }

    public DataModelMetadataBuilder<TEntity> WithPrimaryKey(params string[] propertyNames)
    {
        _primaryKey.Clear();
        _primaryKey.AddRange(propertyNames);
        return this;
    }

    public DataModelMetadataBuilder<TEntity> AddIndex(params string[] propertyNames)
    {
        _indexes.Add(propertyNames);
        return this;
    }

    public DataModelMetadataBuilder<TEntity> AddUniqueConstraint(params string[] propertyNames)
    {
        _uniqueConstraints.Add(propertyNames);
        return this;
    }

    public DataModelMetadataBuilder<TEntity> AddRelationship(IRelationshipMetadata relationship)
    {
        _relationships.Add(relationship);
        return this;
    }

    public DataModelMetadataBuilder<TEntity> AddRelationships(params IRelationshipMetadata[] relationships)
    {
        _relationships.AddRange(relationships);
        return this;
    }

    public DataModelMetadataBuilder<TEntity> WithSoftDelete()
    {
        _usesSoftDelete = true;
        return this;
    }

    public DataModelMetadataBuilder<TEntity> WithAuditing()
    {
        _isAuditable = true;
        return this;
    }

    public DataModelMetadataBuilder<TEntity> WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public DataModelMetadataBuilder<TEntity> WithCustomMetadata(string key, object value)
    {
        _customMetadata ??= new Dictionary<string, object>();
        _customMetadata[key] = value;
        return this;
    }

    public DataModelMetadata<TEntity> Build()
    {
        return new DataModelMetadata<TEntity>(
            _entityName,
            _displayName,
            _tableName,
            _schema,
            _properties.ToFrozenDictionary(),
            _primaryKey,
            _indexes,
            _uniqueConstraints,
            _relationships,
            _usesSoftDelete,
            _isAuditable,
            _description,
            _customMetadata?.ToFrozenDictionary());
    }
}
