namespace Inventorization.Base.Models;

/// <summary>
/// Base entity interface for all domain entities
/// </summary>
public interface IEntity<TPrimaryKey> where TPrimaryKey : struct
{
    TPrimaryKey Id { get; }
}

/// <summary>
/// Base entity class with generic primary key support (int, Guid, etc.)
/// </summary>
public abstract class BaseEntity<TPrimaryKey> : IEntity<TPrimaryKey>
    where TPrimaryKey : struct
{
    public TPrimaryKey Id { get; protected set; }
}

/// <summary>
/// Convenience base entity for Guid primary keys (most common case)
/// </summary>
public abstract class BaseEntity : BaseEntity<Guid>
{
}
