using Microsoft.EntityFrameworkCore;

namespace Inventorization.Base.DataAccess;

/// <summary>
/// Base repository implementation providing generic CRUD operations for any entity
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly DbContext Context;

    public BaseRepository(DbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Retrieves an entity by its ID
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>().FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves all entities
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Creates and adds a new entity
    /// </summary>
    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        await Context.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        Context.Set<T>().Update(entity);
        return await Task.FromResult(entity);
    }

    /// <summary>
    /// Deletes an entity by ID
    /// </summary>
    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        
        Context.Set<T>().Remove(entity);
        return true;
    }

    /// <summary>
    /// Gets a queryable set for LINQ queries
    /// </summary>
    public virtual IQueryable<T> GetQueryable()
    {
        return Context.Set<T>();
    }
}
