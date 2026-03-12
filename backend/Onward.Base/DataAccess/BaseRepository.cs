using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Onward.Base.DataAccess;

/// <summary>
/// Generic base repository implementation for entities with arbitrary primary-key types.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TKey">Primary-key type</typeparam>
public class BaseRepository<T, TKey> : IRepository<T, TKey> where T : class
{
    protected readonly DbContext Context;

    public BaseRepository(DbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>Retrieves an entity by its primary key.</summary>
    public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>().FindAsync(new object[] { id! }, cancellationToken: cancellationToken);
    }

    /// <summary>Retrieves all entities.</summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>().ToListAsync(cancellationToken);
    }

    /// <summary>Creates and adds a new entity.</summary>
    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await Context.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>Updates an existing entity.</summary>
    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        Context.Set<T>().Update(entity);
        return await Task.FromResult(entity);
    }

    /// <summary>Deletes an entity by its primary key.</summary>
    public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        Context.Set<T>().Remove(entity);
        return true;
    }

    /// <summary>Checks whether an entity with the given key exists.</summary>
    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }

    /// <summary>Finds entities matching the given predicate.</summary>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return await Context.Set<T>().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Removes an entity that is already tracked by the context.
    /// When the entity implements <see cref="Onward.Base.Models.IVersionedEntity"/>, EF Core
    /// automatically adds the concurrency token to the DELETE WHERE clause.
    /// </summary>
    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        Context.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>Gets a queryable set for LINQ queries.</summary>
    public virtual IQueryable<T> GetQueryable()
    {
        return Context.Set<T>();
    }
}

/// <summary>
/// Backward-compatible Guid-keyed repository. Alias for <see cref="BaseRepository{T,Guid}"/>.
/// </summary>
public class BaseRepository<T> : BaseRepository<T, Guid>, IRepository<T> where T : class
{
    public BaseRepository(DbContext context) : base(context) { }
}
