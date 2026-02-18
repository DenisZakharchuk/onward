using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Inventorization.Base.Models;
using Inventorization.Base.Abstractions;

namespace Inventorization.Base.DataAccess;

/// <summary>
/// Base configuration for junction entities (many-to-many relationships).
/// Handles composite unique index and provides access to relationship metadata.
/// </summary>
/// <typeparam name="TJunction">Junction entity type (must inherit from JunctionEntityBase)</typeparam>
/// <typeparam name="TEntity">Primary entity type in the relationship</typeparam>
/// <typeparam name="TRelatedEntity">Related entity type in the relationship</typeparam>
public abstract class JunctionEntityConfiguration<TJunction, TEntity, TRelatedEntity> 
    : BaseEntityConfiguration<TJunction>
    where TJunction : JunctionEntityBase
    where TEntity : BaseEntity<Guid>
    where TRelatedEntity : BaseEntity<Guid>
{
    /// <summary>
    /// Relationship metadata describing the many-to-many relationship.
    /// Available to derived classes for configuration.
    /// </summary>
    protected readonly IRelationshipMetadata<TEntity, TRelatedEntity> Metadata;
    
    /// <summary>
    /// Creates a junction entity configuration with the specified relationship metadata
    /// </summary>
    /// <param name="metadata">Relationship metadata from DataModelRelationships static class</param>
    protected JunctionEntityConfiguration(IRelationshipMetadata<TEntity, TRelatedEntity> metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        
        // Validate metadata type
        if (Metadata.Type != RelationshipType.ManyToMany)
            throw new ArgumentException(
                $"JunctionEntityConfiguration requires ManyToMany relationship type, got {Metadata.Type}",
                nameof(metadata));
    }
    
    protected override void ConfigureEntity(EntityTypeBuilder<TJunction> builder)
    {
        // Composite unique index on EntityId + RelatedEntityId
        // This prevents duplicate relationships
        builder.HasIndex(e => new { e.EntityId, e.RelatedEntityId })
            .IsUnique();
        
        // Call derived class to configure junction-specific properties and relationships
        ConfigureJunctionEntity(builder);
    }
    
    /// <summary>
    /// Override to configure junction-specific properties (e.g., metadata columns like prices, dates)
    /// and entity relationships. Metadata is available via protected Metadata property.
    /// </summary>
    /// <param name="builder">Entity type builder for the junction entity</param>
    protected abstract void ConfigureJunctionEntity(EntityTypeBuilder<TJunction> builder);
}
