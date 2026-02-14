using System.Text.Json.Serialization;
using Inventorization.Base.ADTs.Converters;

namespace Inventorization.Base.ADTs;

/// <summary>
/// Specifies which fields to include in the query result.
/// Supports selecting entity properties and related entity fields.
/// </summary>
[JsonConverter(typeof(ProjectionRequestConverter))]
public sealed record ProjectionRequest
{
    /// <summary>
    /// Explicit list of fields to project. Empty when IsAllFields is true.
    /// </summary>
    public IReadOnlyList<FieldProjection> Fields { get; init; }
    
    /// <summary>
    /// When true, projects all fields of the entity.
    /// </summary>
    public bool IsAllFields { get; init; }
    
    /// <summary>
    /// When IsAllFields is true, controls whether to include related entities deeply (nested).
    /// </summary>
    public bool IncludeRelatedDeep { get; init; }
    
    /// <summary>
    /// Maximum depth of nested projections to include (1-7, default 1).
    /// Only used when IncludeRelatedDeep is true.
    /// </summary>
    public int Depth { get; init; }
    
    /// <summary>
    /// Field transformations mapping output field names to projection field transformations.
    /// When specified, this takes precedence over simple Fields array.
    /// </summary>
    public IReadOnlyDictionary<string, ProjectionField>? FieldTransformations { get; init; }
    
    /// <summary>
    /// Maximum allowed depth for nested projections
    /// </summary>
    public const int MaxDepth = 7;
    
    /// <summary>
    /// Default depth for nested projections
    /// </summary>
    public const int DefaultDepth = 1;
    
    public ProjectionRequest(
        IReadOnlyList<FieldProjection> fields, 
        bool isAllFields = false, 
        bool includeRelatedDeep = false, 
        int depth = DefaultDepth,
        IReadOnlyDictionary<string, ProjectionField>? fieldTransformations = null)
    {
        Fields = fields ?? Array.Empty<FieldProjection>();
        IsAllFields = isAllFields;
        IncludeRelatedDeep = includeRelatedDeep;
        Depth = Math.Clamp(depth, 1, MaxDepth);
        FieldTransformations = fieldTransformations;
    }
    
    /// <summary>
    /// Convenience constructor for params-style initialization
    /// </summary>
    public ProjectionRequest(params FieldProjection[] fields) : this((IReadOnlyList<FieldProjection>)fields, false, false, DefaultDepth) { }
    
    /// <summary>
    /// Creates a projection request with all default fields (non-deep)
    /// </summary>
    public static ProjectionRequest Default() => new(Array.Empty<FieldProjection>(), isAllFields: true, includeRelatedDeep: false, depth: DefaultDepth);
    
    /// <summary>
    /// Creates a projection request with all fields including related entities deeply
    /// </summary>
    /// <param name="depth">Maximum depth of nested projections (1-7, default 1)</param>
    public static ProjectionRequest AllDeep(int depth = DefaultDepth) => new(Array.Empty<FieldProjection>(), isAllFields: true, includeRelatedDeep: true, depth: depth);
    
    /// <summary>
    /// Creates a projection request with all direct fields only (no related entities)
    /// </summary>
    public static ProjectionRequest AllDirect() => new(Array.Empty<FieldProjection>(), isAllFields: true, includeRelatedDeep: false, depth: DefaultDepth);
}

/// <summary>
/// Represents a single field to project in the result.
/// Can be a direct entity field or a related entity field via navigation property.
/// </summary>
public sealed record FieldProjection
{
    /// <summary>
    /// Name of the field to project (e.g., "Name", "Category.Name")
    /// </summary>
    public string FieldName { get; init; }
    
    /// <summary>
    /// Whether this field is from a related entity
    /// </summary>
    public bool IsRelated { get; init; }
    
    /// <summary>
    /// Path to the related entity (e.g., "Category", "Supplier.ContactPerson")
    /// Only applicable when IsRelated is true
    /// </summary>
    public string? RelationPath { get; init; }
    
    public FieldProjection(string fieldName, bool isRelated = false, string? relationPath = null)
    {
        FieldName = fieldName;
        IsRelated = isRelated;
        RelationPath = relationPath;
    }
}
