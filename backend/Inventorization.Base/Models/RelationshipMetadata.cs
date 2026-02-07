using Inventorization.Base.Abstractions;

namespace Inventorization.Base.Models;

/// <summary>
/// Generic metadata describing a relationship between two entity types.
/// Implements IRelationshipMetadata to support dependency inversion.
/// </summary>
/// <typeparam name="TEntity">The primary entity type in the relationship</typeparam>
/// <typeparam name="TRelatedEntity">The related entity type in the relationship</typeparam>
public class RelationshipMetadata<TEntity, TRelatedEntity> : IRelationshipMetadata<TEntity, TRelatedEntity>
{
    /// <summary>
    /// Type of relationship (OneToOne, OneToMany, ManyToMany)
    /// </summary>
    public RelationshipType Type { get; }

    /// <summary>
    /// Cardinality constraint for the relationship
    /// </summary>
    public RelationshipCardinality Cardinality { get; }

    /// <summary>
    /// Name of the primary entity in the relationship
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Name of the related entity in the relationship
    /// </summary>
    public string RelatedEntityName { get; }

    /// <summary>
    /// Name of the junction entity (for ManyToMany relationships only)
    /// </summary>
    public string? JunctionEntityName { get; }

    /// <summary>
    /// Display name for the relationship (e.g., "User Roles", "Order Items")
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Optional description of the relationship's business purpose
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Name of the navigation property on the entity.
    /// Required for handling multiple relationships to the same entity type.
    /// </summary>
    public string? NavigationPropertyName { get; }

    public RelationshipMetadata(
        RelationshipType type,
        RelationshipCardinality cardinality,
        string entityName,
        string relatedEntityName,
        string displayName,
        string? junctionEntityName = null,
        string? navigationPropertyName = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Entity name cannot be null or empty", nameof(entityName));
        if (string.IsNullOrWhiteSpace(relatedEntityName))
            throw new ArgumentException("Related entity name cannot be null or empty", nameof(relatedEntityName));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be null or empty", nameof(displayName));

        if (type == RelationshipType.ManyToMany && string.IsNullOrWhiteSpace(junctionEntityName))
            throw new ArgumentException("Junction entity name is required for ManyToMany relationships", nameof(junctionEntityName));

        Type = type;
        Cardinality = cardinality;
        EntityName = entityName;
        RelatedEntityName = relatedEntityName;
        JunctionEntityName = junctionEntityName;
        DisplayName = displayName;
        NavigationPropertyName = navigationPropertyName;
        Description = description;
    }

    public override string ToString()
    {
        var relationship = Type switch
        {
            RelationshipType.OneToOne => $"{EntityName} ↔ {RelatedEntityName}",
            RelationshipType.OneToMany => $"{EntityName} → {RelatedEntityName}",
            RelationshipType.ManyToMany => $"{EntityName} ↔ {RelatedEntityName} (via {JunctionEntityName})",
            _ => $"{EntityName} - {RelatedEntityName}"
        };

        var nav = NavigationPropertyName != null ? $" [{NavigationPropertyName}]" : "";
        return $"{DisplayName}: {relationship} ({Cardinality}){nav}";
    }
}
