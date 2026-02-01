namespace Inventorization.Base.DataAccess;

/// <summary>
/// Generic repository interface for common CRUD operations.
/// This abstraction allows for multiple storage implementations (SQL, NoSQL, file-based, etc.)
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Returns an IQueryable for LINQ filtering and projection
    /// </summary>
    IQueryable<T> GetQueryable();
}
