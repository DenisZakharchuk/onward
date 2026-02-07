namespace Inventorization.Base.Models;

/// <summary>
/// Base class for many-to-many junction entities.
/// Provides standardized EntityId and RelatedEntityId properties with validated constructor.
/// </summary>
public abstract class JunctionEntityBase : BaseEntity
{
    /// <summary>
    /// Parameterless constructor for EF Core only.
    /// </summary>
    protected JunctionEntityBase()
    {
    }

    /// <summary>
    /// Creates a junction entity with validated foreign keys.
    /// </summary>
    /// <param name="entityId">Parent entity ID</param>
    /// <param name="relatedEntityId">Related entity ID</param>
    /// <exception cref="ArgumentException">Thrown if either ID is empty</exception>
    protected JunctionEntityBase(Guid entityId, Guid relatedEntityId)
    {
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));
        if (relatedEntityId == Guid.Empty)
            throw new ArgumentException("Related entity ID cannot be empty", nameof(relatedEntityId));

        EntityId = entityId;
        RelatedEntityId = relatedEntityId;
    }

    /// <summary>
    /// Foreign key to the parent entity.
    /// </summary>
    public Guid EntityId { get; protected set; }

    /// <summary>
    /// Foreign key to the related entity.
    /// </summary>
    public Guid RelatedEntityId { get; protected set; }
}
