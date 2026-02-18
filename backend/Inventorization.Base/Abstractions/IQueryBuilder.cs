using Inventorization.Base.ADTs;
using Inventorization.Base.DTOs;
using Inventorization.Base.Models;
using Inventorization.Base.Ownership;
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
/// Ownership-aware extension of <see cref="IQueryBuilder{TEntity}"/>.
/// The builder is injected with an <see cref="ICurrentIdentityContext{TOwnership}"/>
/// and exposes an ownership-scoped query path in addition to the standard path.
/// This keeps ownership filtering inside the query pipeline rather than as a
/// global EF query filter, giving each bounded context full control over the predicate.
/// </summary>
/// <typeparam name="TEntity">The entity type to build queries for.</typeparam>
/// <typeparam name="TOwnership">Concrete ownership VO for this bounded context.</typeparam>
public interface IQueryBuilder<TEntity, TOwnership>
    : IQueryBuilder<TEntity>
    where TEntity : class
    where TOwnership : OwnershipValueObject
{
    /// <summary>
    /// Builds a queryable that applies all standard <see cref="SearchQuery"/> clauses
    /// AND an additional ownership predicate derived from the injected
    /// <see cref="ICurrentIdentityContext{TOwnership}"/>.
    /// When the caller is not authenticated, the predicate evaluates to no results
    /// (returns an empty queryable) unless the entity does not implement
    /// <see cref="IOwnedEntity{TOwnership}"/>, in which case the standard query is returned.
    /// </summary>
    IQueryable<TEntity> BuildOwnedQuery(IQueryable<TEntity> baseQuery, SearchQuery searchQuery);
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

/// <summary>
/// Ownership-aware extension of <see cref="ISearchService{TEntity,TProjection}"/>.
/// Adds an ownership-scoped search method that automatically applies the caller's
/// ownership predicate to the query, driven by the injected
/// <see cref="ICurrentIdentityContext{TOwnership}"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type to search.</typeparam>
/// <typeparam name="TProjection">The projection result type.</typeparam>
/// <typeparam name="TOwnership">Concrete ownership VO for this bounded context.</typeparam>
public interface ISearchService<TEntity, TProjection, TOwnership>
    : ISearchService<TEntity, TProjection>
    where TEntity : class
    where TProjection : class
    where TOwnership : OwnershipValueObject
{
    /// <summary>
    /// Executes a paginated search scoped to the current caller's ownership context.
    /// The ownership predicate is appended by the injected
    /// <see cref="IQueryBuilder{TEntity,TOwnership}"/> implementation.
    /// </summary>
    Task<ServiceResult<SearchResult<TProjection>>> ExecuteOwnedSearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default);
}
