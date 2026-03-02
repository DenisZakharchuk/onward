using System.Linq.Expressions;

namespace Onward.Base.DataAccess;

/// <summary>
/// Generic repository interface for common CRUD operations with a configurable primary key type.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type</typeparam>
public interface IRepository<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists by ID
    /// </summary>
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the given predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an IQueryable for LINQ filtering and projection
    /// </summary>
    IQueryable<T> GetQueryable();
}

/// <summary>
/// Generic repository interface for common CRUD operations — convenience Guid alias.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> : IRepository<T, Guid> where T : class
{
}
