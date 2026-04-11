namespace Onward.Base.Sql;

/// <summary>
/// Executes pre-built <see cref="SqlQuery"/> objects against a data store and returns
/// materialized entity instances.
/// Decouples <see cref="BaseSqlSearchService{TEntity,TProjection}"/> from the specific
/// data-access technology (EF Core vs ADO.NET).
/// </summary>
/// <typeparam name="TEntity">The entity type to materialize.</typeparam>
public interface ISearchDataExecutor<TEntity> where TEntity : class
{
    /// <summary>Executes a SELECT query and returns the matching rows as entities.</summary>
    Task<IReadOnlyList<TEntity>> FetchAsync(SqlQuery query, CancellationToken cancellationToken);

    /// <summary>Executes a COUNT query and returns the scalar result.</summary>
    Task<int> CountAsync(SqlQuery query, CancellationToken cancellationToken);
}
