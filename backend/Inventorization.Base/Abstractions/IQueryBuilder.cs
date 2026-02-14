using Inventorization.Base.ADTs;
using Inventorization.Base.DTOs;
using Inventorization.Base.Models;
using System.Linq.Expressions;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Builds IQueryable from SearchQuery ADT.
/// Converts ADT filter expressions to LINQ expressions for database queries.
/// </summary>
public interface IQueryBuilder<TEntity> where TEntity : class
{
    /// <summary>
    /// Converts FilterExpression ADT to LINQ Where expression
    /// </summary>
    Expression<Func<TEntity, bool>>? BuildFilterExpression(FilterExpression? filter);
    
    /// <summary>
    /// Applies projection to queryable (Select clause with includes)
    /// </summary>
    IQueryable<TEntity> ApplyProjection(IQueryable<TEntity> query, ProjectionRequest? projection);
    
    /// <summary>
    /// Applies sorting to queryable (OrderBy/ThenBy clauses)
    /// </summary>
    IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, SortRequest? sort);
    
    /// <summary>
    /// Builds complete queryable from SearchQuery ADT
    /// </summary>
    IQueryable<TEntity> BuildQuery(IQueryable<TEntity> baseQuery, SearchQuery searchQuery);
}

/// <summary>
/// Service for executing ADT-based search queries.
/// Replaces the old ISearchQueryProvider pattern.
/// Supports both regular projections and field transformations.
/// </summary>
public interface ISearchService<TEntity, TProjection> 
    where TEntity : class
    where TProjection : class
{
    /// <summary>
    /// Executes a search query and returns paginated results
    /// </summary>
    Task<ServiceResult<SearchResult<TProjection>>> ExecuteSearchAsync(
        SearchQuery query, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a search query with field transformations, returning TransformationResult.
    /// Field transformations allow computed fields, string operations, arithmetic, conditionals, etc.
    /// </summary>
    Task<ServiceResult<SearchResult<TransformationResult>>> ExecuteTransformationSearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default);
}
