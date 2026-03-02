namespace Onward.Base.Models;

/// <summary>
/// Base class for many-to-many junction entities with configurable left/right FK types.
/// Provides standardized EntityId and RelatedEntityId properties with validated constructor.
/// </summary>
public abstract class JunctionEntityBase<TLeftKey, TRightKey> : BaseEntity
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
    protected JunctionEntityBase(TLeftKey entityId, TRightKey relatedEntityId)
    {
        if (EqualityComparer<TLeftKey>.Default.Equals(entityId, default!))
            throw new ArgumentException("Entity ID cannot be default/empty", nameof(entityId));
        if (EqualityComparer<TRightKey>.Default.Equals(relatedEntityId, default!))
            throw new ArgumentException("Related entity ID cannot be default/empty", nameof(relatedEntityId));

        EntityId = entityId;
        RelatedEntityId = relatedEntityId;
    }

    /// <summary>
    /// Foreign key to the parent entity.
    /// </summary>
    public TLeftKey EntityId { get; protected set; } = default!;

    /// <summary>
    /// Foreign key to the related entity.
    /// </summary>
    public TRightKey RelatedEntityId { get; protected set; } = default!;
}

/// <summary>
/// Base class for many-to-many junction entities with Guid FK columns — convenience alias.
/// </summary>
public abstract class JunctionEntityBase : JunctionEntityBase<Guid, Guid>
{
    protected JunctionEntityBase() : base() { }

    protected JunctionEntityBase(Guid entityId, Guid relatedEntityId)
        : base(entityId, relatedEntityId) { }
}
