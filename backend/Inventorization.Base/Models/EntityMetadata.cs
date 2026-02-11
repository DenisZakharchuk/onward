namespace Inventorization.Base.Models;

/// <summary>
/// Metadata describing an entity's structure, validation rules, and configuration.
/// String-based and self-contained - does not reference actual entity types.
/// </summary>
public class EntityMetadata
{
    public string EntityName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string? SchemaName { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool HasAuditing { get; set; }
    public PropertyMetadata[] Properties { get; set; } = Array.Empty<PropertyMetadata>();
    public IndexMetadata[] Indexes { get; set; } = Array.Empty<IndexMetadata>();
    public UniqueConstraintMetadata[] UniqueConstraints { get; set; } = Array.Empty<UniqueConstraintMetadata>();
}

/// <summary>
/// Metadata describing a single property of an entity.
/// </summary>
public class PropertyMetadata
{
    public string PropertyName { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsPrimaryKey { get; set; }
    public bool IsRequired { get; set; }
    public bool IsNullable { get; set; }
    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public string? ColumnType { get; set; }
    public bool IsUnique { get; set; }
    public bool IsForeignKey { get; set; }
    public bool IsIndexed { get; set; }
    public object? MinValue { get; set; }
    public object? MaxValue { get; set; }
    public string? RegexPattern { get; set; }
    public string? DefaultValue { get; set; }
    public string? DefaultValueSql { get; set; }
    public bool IsEmail { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ValidationMessage { get; set; }
}

/// <summary>
/// Metadata describing a database index.
/// </summary>
public class IndexMetadata
{
    public string Name { get; set; } = string.Empty;
    public string[] Columns { get; set; } = Array.Empty<string>();
    public bool IsUnique { get; set; }
}

/// <summary>
/// Metadata describing a unique constraint.
/// </summary>
public class UniqueConstraintMetadata
{
    public string Name { get; set; } = string.Empty;
    public string[] Columns { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Metadata describing a relationship between two entities.
/// String-based and self-contained - does not reference actual entity types.
/// </summary>
public class RelationshipMetadata
{
    public string RelationshipName { get; set; } = string.Empty;
    public RelationshipType Type { get; set; }
    public RelationshipCardinality Cardinality { get; set; }
    public string PrincipalEntity { get; set; } = string.Empty;
    public string DependentEntity { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? JunctionEntityName { get; set; }
    public string? NavigationPropertyName { get; set; }
    public string? InverseNavigationPropertyName { get; set; }
    public string? ForeignKeyPropertyName { get; set; }
    public string Description { get; set; } = string.Empty;
}
