namespace Onward.Base.Models;

/// <summary>
/// Interface for entities that support soft deletion
/// </summary>
public interface ISoftDeletableEntity
{
    /// <summary>
    /// Indicates whether the entity is soft-deleted
    /// </summary>
    bool IsDeleted { get; }
    
    /// <summary>
    /// Timestamp when the entity was soft-deleted
    /// </summary>
    DateTime? DeletedAt { get; }
    
    /// <summary>
    /// Marks the entity as deleted
    /// </summary>
    void MarkAsDeleted();
    
    /// <summary>
    /// Restores a soft-deleted entity
    /// </summary>
    void Restore();
}

/// <summary>
/// Base entity interface for all domain entities
/// </summary>
public interface IEntity<TPrimaryKey>
{
    TPrimaryKey Id { get; }
}

/// <summary>
/// Base entity class with generic primary key support (int, Guid, string, etc.)
/// </summary>
public abstract class BaseEntity<TPrimaryKey> : IEntity<TPrimaryKey>
{
    public TPrimaryKey Id { get; protected set; } = default!;
}

/// <summary>
/// Convenience base entity for Guid primary keys (most common case)
/// </summary>
public abstract class BaseEntity : BaseEntity<Guid>
{
}
