namespace Onward.Base.Models;

/// <summary>
/// Marks entities that participate in optimistic concurrency control via a row version token.
/// The version token is managed entirely by the database (e.g. PostgreSQL xmin, SQL Server rowversion).
/// Entities implement this interface when <c>versioned: 'rowversion'</c> is declared in the data model.
/// </summary>
/// <remarks>
/// Intentionally kept as a standalone marker so only opted-in entities pay the cost —
/// <see cref="BaseEntity{TPrimaryKey}"/> does NOT implement this interface.
/// The <c>uint</c> type maps directly to PostgreSQL's <c>xid</c> / <c>xmin</c> column type.
/// Future versioning strategies (timestamp, content hash) should add new discriminant modes
/// rather than extending this interface.
/// </remarks>
public interface IVersionedEntity
{
    uint RowVersion { get; set; }
}

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
