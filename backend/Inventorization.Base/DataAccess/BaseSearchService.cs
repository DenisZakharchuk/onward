using Inventorization.Base.Abstractions;
using Inventorization.Base.ADTs;
using Inventorization.Base.DTOs;
using Inventorization.Base.Models;
using Inventorization.Base.Ownership;
using Inventorization.Base.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventorization.Base.DataAccess;

/// <summary>
/// Base implementation for executing ADT-based search queries on entities.
/// Implements the ISearchService pattern for flexible querying.
/// Supports both regular projections and field transformations.
/// </summary>
/// <typeparam name="TEntity">The entity type to search</typeparam>
/// <typeparam name="TProjection">The projection result type</typeparam>
public abstract class BaseSearchService<TEntity, TProjection> : ISearchService<TEntity, TProjection>
    where TEntity : class
    where TProjection : class, new()
{
    protected readonly IRepository<TEntity> Repository;
    protected readonly IQueryBuilder<TEntity> QueryBuilder;
    protected readonly IProjectionMapper<TEntity, TProjection> ProjectionMapper;
    protected readonly ProjectionExpressionBuilder ExpressionBuilder;
    protected readonly IValidator<SearchQuery> Validator;
    protected readonly ILogger Logger;
    
    protected BaseSearchService(
        IRepository<TEntity> repository,
        IQueryBuilder<TEntity> queryBuilder,
        IProjectionMapper<TEntity, TProjection> projectionMapper,
        ProjectionExpressionBuilder expressionBuilder,
        IValidator<SearchQuery> validator,
        ILogger logger)
    {
        Repository = repository;
        QueryBuilder = queryBuilder;
        ProjectionMapper = projectionMapper;
        ExpressionBuilder = expressionBuilder;
        Validator = validator;
        Logger = logger;
    }
    
    /// <summary>
    /// Gets the entity type name for logging purposes
    /// </summary>
    protected virtual string EntityName => typeof(TEntity).Name;
    
    public virtual async Task<ServiceResult<SearchResult<TProjection>>> ExecuteSearchAsync(
        SearchQuery query, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate the search query
            var validationResult = await Validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                Logger.LogWarning("Search query validation failed for {EntityName}: {Errors}", 
                    EntityName, string.Join(", ", validationResult.Errors));
                return ServiceResult<SearchResult<TProjection>>.Failure(
                    "Search query validation failed", 
                    validationResult.Errors);
            }
            
            // Get base queryable
            var baseQuery = Repository.GetQueryable();
            
            // Build query using query builder
            var builtQuery = QueryBuilder.BuildQuery(baseQuery, query);
            
            // Get total count before pagination
            var totalCount = await builtQuery.CountAsync(cancellationToken);
            
            // Apply pagination
            var pagedQuery = builtQuery
                .Skip((query.Pagination.PageNumber - 1) * query.Pagination.PageSize)
                .Take(query.Pagination.PageSize);
            
            // Execute query with projection
            var projectionExpression = ProjectionMapper.GetProjectionExpression(query.Projection);
            var items = await pagedQuery
                .Select(projectionExpression)
                .ToListAsync(cancellationToken);
            
            // Build result
            var result = new SearchResult<TProjection>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.Pagination.PageNumber,
                PageSize = query.Pagination.PageSize
            };
            
            Logger.LogInformation(
                "{EntityName} search executed successfully: {TotalCount} total items, returned page {PageNumber} with {ItemCount} items",
                EntityName, totalCount, query.Pagination.PageNumber, items.Count);
            
            return ServiceResult<SearchResult<TProjection>>.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing search query for {EntityName}", EntityName);
            return ServiceResult<SearchResult<TProjection>>.Failure(
                $"Failed to execute search: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a search query with field transformations, returning TransformationResult.
    /// Field transformations allow computed fields, string operations, arithmetic, conditionals, etc.
    /// </summary>
    public virtual async Task<ServiceResult<SearchResult<TransformationResult>>> ExecuteTransformationSearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate query has transformations
            if (query.Projection?.FieldTransformations == null || query.Projection.FieldTransformations.Count == 0)
            {
                Logger.LogWarning("Transformation search called without field transformations for {EntityName}", EntityName);
                return ServiceResult<SearchResult<TransformationResult>>.Failure(
                    "Field transformations are required for transformation search. Use ExecuteSearchAsync for regular projections.");
            }

            // Validate the search query
            var validationResult = await Validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                Logger.LogWarning("Transformation search query validation failed for {EntityName}: {Errors}",
                    EntityName, string.Join(", ", validationResult.Errors));
                return ServiceResult<SearchResult<TransformationResult>>.Failure(
                    "Search query validation failed",
                    validationResult.Errors);
            }

            // Get base queryable
            var baseQuery = Repository.GetQueryable();

            // Build query using query builder (filters, sorting)
            var builtQuery = QueryBuilder.BuildQuery(baseQuery, query);

            // Get total count before pagination
            var totalCount = await builtQuery.CountAsync(cancellationToken);

            // Apply pagination
            var pagedQuery = builtQuery
                .Skip((query.Pagination.PageNumber - 1) * query.Pagination.PageSize)
                .Take(query.Pagination.PageSize);

            // Load entities into memory first
            var entities = await pagedQuery.ToListAsync(cancellationToken);

            // Build schema from transformations
            var schema = new Dictionary<string, Type>(15);
            foreach (var kvp in query.Projection.FieldTransformations)
            {
                schema[kvp.Key] = kvp.Value.GetOutputType();
            }

            // Build transformation expression for in-memory evaluation
            var transformationExpression = ExpressionBuilder.BuildTransformationExpression<TEntity>(
                query.Projection.FieldTransformations);
            var compiledExpression = transformationExpression.Compile();

            // Apply transformations in memory
            var items = new List<TransformationResult>(entities.Count);
            foreach (var entity in entities)
                items.Add(compiledExpression(entity));

            // Build result
            var result = new SearchResult<TransformationResult>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.Pagination.PageNumber,
                PageSize = query.Pagination.PageSize
            };

            Logger.LogInformation(
                "{EntityName} transformation search executed successfully: {TotalCount} total items, returned page {PageNumber} with {ItemCount} items, {TransformationCount} transformations",
                EntityName, totalCount, query.Pagination.PageNumber, items.Count, query.Projection.FieldTransformations.Count);

            return ServiceResult<SearchResult<TransformationResult>>.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing transformation search query for {EntityName}", EntityName);
            return ServiceResult<SearchResult<TransformationResult>>.Failure(
                $"Failed to execute transformation search: {ex.Message}");
        }
    }
}

/// <summary>
/// Ownership-aware base search service.
/// Extends <see cref="BaseSearchService{TEntity,TProjection}"/> with
/// <see cref="ExecuteOwnedSearchAsync"/> which delegates to
/// <see cref="IQueryBuilder{TEntity,TOwnership}.BuildOwnedQuery"/> instead of the
/// standard <see cref="IQueryBuilder{TEntity}.BuildQuery"/>.
/// </summary>
/// <remarks>
/// The <see cref="ICurrentIdentityContext{TOwnership}"/> is injected here so bounded
/// contexts can log or inspect the identity during the search pipeline without
/// duplicating the dependency resolution.
/// </remarks>
/// <typeparam name="TEntity">The entity type to search.</typeparam>
/// <typeparam name="TProjection">The projection result type.</typeparam>
/// <typeparam name="TOwnership">Concrete ownership VO for this bounded context.</typeparam>
public abstract class BaseSearchService<TEntity, TProjection, TOwnership>
    : BaseSearchService<TEntity, TProjection>,
      ISearchService<TEntity, TProjection, TOwnership>
    where TEntity : class
    where TProjection : class, new()
    where TOwnership : OwnershipValueObject
{
    private readonly IQueryBuilder<TEntity, TOwnership> _ownedQueryBuilder;
    private readonly ICurrentIdentityContext<TOwnership> _identityContext;

    protected BaseSearchService(
        IRepository<TEntity> repository,
        IQueryBuilder<TEntity, TOwnership> ownedQueryBuilder,
        IProjectionMapper<TEntity, TProjection> projectionMapper,
        ProjectionExpressionBuilder expressionBuilder,
        IValidator<SearchQuery> validator,
        ICurrentIdentityContext<TOwnership> identityContext,
        ILogger logger)
        : base(repository, ownedQueryBuilder, projectionMapper, expressionBuilder, validator, logger)
    {
        _ownedQueryBuilder = ownedQueryBuilder ?? throw new ArgumentNullException(nameof(ownedQueryBuilder));
        _identityContext = identityContext ?? throw new ArgumentNullException(nameof(identityContext));
    }

    /// <inheritdoc />
    public virtual async Task<ServiceResult<SearchResult<TProjection>>> ExecuteOwnedSearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate the search query
            var validationResult = await Validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                Logger.LogWarning("Owned search query validation failed for {EntityName}: {Errors}",
                    EntityName, string.Join(", ", validationResult.Errors));
                return ServiceResult<SearchResult<TProjection>>.Failure(
                    "Search query validation failed",
                    validationResult.Errors);
            }

            // Build owned query — includes both standard filters and ownership predicate
            var baseQuery = Repository.GetQueryable();
            var builtQuery = _ownedQueryBuilder.BuildOwnedQuery(baseQuery, query);

            // Get total count before pagination
            var totalCount = await builtQuery.CountAsync(cancellationToken);

            // Apply pagination
            var pagedQuery = builtQuery
                .Skip((query.Pagination.PageNumber - 1) * query.Pagination.PageSize)
                .Take(query.Pagination.PageSize);

            // Execute query with projection
            var projectionExpression = ProjectionMapper.GetProjectionExpression(query.Projection);
            var items = await pagedQuery
                .Select(projectionExpression)
                .ToListAsync(cancellationToken);

            var result = new SearchResult<TProjection>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.Pagination.PageNumber,
                PageSize = query.Pagination.PageSize
            };

            Logger.LogInformation(
                "{EntityName} owned search executed for caller {OwnershipSummary}: {TotalCount} total items, page {PageNumber} with {ItemCount} items",
                EntityName, _identityContext.Ownership?.ToString() ?? "anonymous",
                totalCount, query.Pagination.PageNumber, items.Count);

            return ServiceResult<SearchResult<TProjection>>.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing owned search query for {EntityName}", EntityName);
            return ServiceResult<SearchResult<TProjection>>.Failure(
                $"Failed to execute owned search: {ex.Message}");
        }
    }
}
