namespace Inventorization.Base.Abstractions;

/// <summary>
/// Metadata for a complete domain entity/data model.
/// Provides comprehensive information for validation, EF configuration, and code generation.
/// </summary>
public interface IDataModelMetadata
{
    /// <summary>
    /// Entity CLR type
    /// </summary>
    Type EntityType { get; }

    /// <summary>
    /// Entity name (typically the class name)
    /// </summary>
    string EntityName { get; }

    /// <summary>
    /// Display name for UI/documentation
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Database table name
    /// </summary>
    string TableName { get; }

    /// <summary>
    /// Database schema name
    /// </summary>
    string? Schema { get; }

    /// <summary>
    /// All property metadata for this entity
    /// </summary>
    IReadOnlyDictionary<string, IDataPropertyMetadata> Properties { get; }

    /// <summary>
    /// Primary key property names
    /// </summary>
    IReadOnlyList<string> PrimaryKey { get; }

    /// <summary>
    /// Indexes defined on this entity (property names)
    /// </summary>
    IReadOnlyList<string[]> Indexes { get; }

    /// <summary>
    /// Unique constraints (property name combinations)
    /// </summary>
    IReadOnlyList<string[]> UniqueConstraints { get; }

    /// <summary>
    /// Relationships this entity participates in
    /// </summary>
    IReadOnlyList<IRelationshipMetadata> Relationships { get; }

    /// <summary>
    /// Whether this entity uses soft delete
    /// </summary>
    bool UsesSoftDelete { get; }

    /// <summary>
    /// Whether this entity is auditable (tracks created/modified)
    /// </summary>
    bool IsAuditable { get; }

    /// <summary>
    /// Description for documentation
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Custom metadata key-value pairs
    /// </summary>
    IReadOnlyDictionary<string, object>? CustomMetadata { get; }
}

/// <summary>
/// Generic version of IDataModelMetadata for type-safe access
/// </summary>
public interface IDataModelMetadata<TEntity> : IDataModelMetadata
    where TEntity : class
{
}
