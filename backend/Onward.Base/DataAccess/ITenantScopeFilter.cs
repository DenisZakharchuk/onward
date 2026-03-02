namespace Onward.Base.DataAccess;

/// <summary>
/// Applies a tenant-scoped filter to an <see cref="IQueryable{TEntity}"/>.
/// <para>
/// Recommended usage: register a concrete implementation per entity in the bounded
/// context's DI configuration; <see cref="Services.DataServiceBase{TOwnership,TEntity,TCreateDTO,TUpdateDTO,TDeleteDTO,TInitDTO,TDetailsDTO,TSearchDTO}"/>
/// will resolve it automatically and apply it before executing queries.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The EF Core entity type being filtered.</typeparam>
public interface ITenantScopeFilter<TEntity> where TEntity : class
{
    /// <summary>
    /// Returns a filtered query restricted to resources owned by (or visible to) the
    /// specified <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="query">The base query to filter.</param>
    /// <param name="tenantId">Tenant identifier taken from the current request context.</param>
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, string tenantId);
}
