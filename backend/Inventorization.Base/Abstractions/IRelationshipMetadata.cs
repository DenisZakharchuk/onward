using Inventorization.Base.Models;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Non-generic base interface for relationship metadata.
/// Allows storage of different relationship types in collections.
/// </summary>
public interface IRelationshipMetadata
{
    /// <summary>
    /// Type of relationship (OneToOne, OneToMany, ManyToMany)
    /// </summary>
    RelationshipType Type { get; }

    /// <summary>
    /// Cardinality constraint for the relationship (Required, Optional)
    /// </summary>
    RelationshipCardinality Cardinality { get; }

    /// <summary>
    /// Name of the primary entity in the relationship
    /// </summary>
    string EntityName { get; }

    /// <summary>
    /// Name of the related entity in the relationship
    /// </summary>
    string RelatedEntityName { get; }

    /// <summary>
    /// Name of the junction entity (for ManyToMany relationships only)
    /// </summary>
    string? JunctionEntityName { get; }

    /// <summary>
    /// Display name for the relationship (e.g., "User Roles", "Order Items")
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Optional description of the relationship's business purpose
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Name of the navigation property on the entity.
    /// </summary>
    string? NavigationPropertyName { get; }
}

/// <summary>
/// Generic interface for relationship metadata between two entities.
/// Ensures dependency inversion - relationship managers depend on abstraction, not concrete implementation.
/// Inherits all properties from base IRelationshipMetadata.
/// </summary>
/// <typeparam name="TEntity">The primary entity type in the relationship</typeparam>
/// <typeparam name="TRelatedEntity">The related entity type in the relationship</typeparam>
public interface IRelationshipMetadata<TEntity, TRelatedEntity> : IRelationshipMetadata
{
    // All properties inherited from IRelationshipMetadata base interface
    // This generic version provides type safety for relationship managers
}
