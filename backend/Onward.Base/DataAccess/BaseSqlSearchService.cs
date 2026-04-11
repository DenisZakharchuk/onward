using Onward.Base.Abstractions;
using Onward.Base.ADTs;
using Onward.Base.DTOs;
using Onward.Base.Models;
using Onward.Base.Services;
using Onward.Base.Sql;
using Microsoft.Extensions.Logging;

namespace Onward.Base.DataAccess;

/// <summary>
/// Base search service that executes pre-built SQL queries rather than LINQ expression trees.
/// Replaces <see cref="BaseSearchService{TEntity,TProjection}"/> as the default base when
/// <c>queryMode: "sql"</c> is set in the blueprint configuration.
/// </summary>
/// <remarks>
/// <para>
/// Query flow:
/// <list type="number">
///   <item><see cref="ISqlQueryBuilder"/> converts the <see cref="SearchQuery"/> ADT into
///     a parameterized <see cref="SqlQuery"/> using <see cref="EntityMetadata"/> for
///     field validation and table/column resolution.</item>
///   <item><see cref="ISearchDataExecutor{TEntity}"/> executes the SQL and returns
///     materialized entity instances.  Swap the executor in DI to switch between
///     EF Core (<see cref="EfCoreSqlDataExecutor{TEntity}"/>) and ADO.NET
///     (<see cref="AdoNetSqlDataExecutor{TEntity}"/>).</item>
///   <item><see cref="IProjectionMapper{TEntity,TProjection}"/> maps entities to the
///     projection DTO in memory.</item>
/// </list>
/// </para>
/// <para>
/// Field transformations (<c>ExecuteTransformationSearchAsync</c>) still run in-memory
/// using <see cref="ProjectionExpressionBuilder"/> — SQL generation for transforms is not
/// supported in v1.
/// </para>
/// </remarks>
/// <typeparam name="TEntity">The entity type to search.</typeparam>
/// <typeparam name="TProjection">The projection result type.</typeparam>
public abstract class BaseSqlSearchService<TEntity, TProjection> : ISearchService<TEntity, TProjection>
    where TEntity : class
    where TProjection : class, new()
{
    protected readonly ISearchDataExecutor<TEntity> DataExecutor;
    protected readonly ISqlQueryBuilder SqlQueryBuilder;
    protected readonly IProjectionMapper<TEntity, TProjection> ProjectionMapper;
    protected readonly ProjectionExpressionBuilder ExpressionBuilder;
    protected readonly IValidator<SearchQuery> Validator;
    protected readonly ILogger Logger;

    protected BaseSqlSearchService(
        ISearchDataExecutor<TEntity> dataExecutor,
        ISqlQueryBuilder sqlQueryBuilder,
        IProjectionMapper<TEntity, TProjection> projectionMapper,
        ProjectionExpressionBuilder expressionBuilder,
        IValidator<SearchQuery> validator,
        ILogger logger)
    {
        DataExecutor = dataExecutor ?? throw new ArgumentNullException(nameof(dataExecutor));
        SqlQueryBuilder = sqlQueryBuilder ?? throw new ArgumentNullException(nameof(sqlQueryBuilder));
        ProjectionMapper = projectionMapper ?? throw new ArgumentNullException(nameof(projectionMapper));
        ExpressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        Validator = validator ?? throw new ArgumentNullException(nameof(validator));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Entity-specific metadata snapshot. Derived classes return the corresponding
    /// static field from the generated <c>DataModelMetadata</c> class:
    /// <code>protected override EntityMetadata EntityMetadata =&gt; DataModelMetadata.Tender;</code>
    /// </summary>
    protected abstract EntityMetadata EntityMetadata { get; }

    /// <summary>Entity type name used in log messages.</summary>
    protected virtual string EntityName => typeof(TEntity).Name;

    /// <inheritdoc/>
    public virtual async Task<ServiceResult<SearchResult<TProjection>>> ExecuteSearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await Validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                Logger.LogWarning("SQL search query validation failed for {EntityName}: {Errors}",
                    EntityName, string.Join(", ", validationResult.Errors));
                return ServiceResult<SearchResult<TProjection>>.Failure(
                    "Search query validation failed", validationResult.Errors);
            }

            var countQuery = SqlQueryBuilder.BuildCountQuery(EntityMetadata, query);
            var selectQuery = SqlQueryBuilder.BuildSelectQuery(EntityMetadata, query);

            var totalCount = await DataExecutor.CountAsync(countQuery, cancellationToken);
            var entities = await DataExecutor.FetchAsync(selectQuery, cancellationToken);

            var items = new List<TProjection>(entities.Count);
            foreach (var entity in entities)
                items.Add(ProjectionMapper.Map(entity, query.Projection));

            var result = new SearchResult<TProjection>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.Pagination.PageNumber,
                PageSize = query.Pagination.PageSize
            };

            Logger.LogInformation(
                "{EntityName} SQL search executed: {TotalCount} total, page {Page} with {Count} items",
                EntityName, totalCount, query.Pagination.PageNumber, items.Count);

            return ServiceResult<SearchResult<TProjection>>.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing SQL search for {EntityName}", EntityName);
            return ServiceResult<SearchResult<TProjection>>.Failure($"Failed to execute search: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Field transformations are evaluated in-memory after fetching entities via SQL.
    /// The SQL query uses the filter and pagination from <paramref name="query"/>;
    /// the <see cref="ProjectionRequest.FieldTransformations"/> are applied afterward.
    /// </remarks>
    public virtual async Task<ServiceResult<SearchResult<TransformationResult>>> ExecuteTransformationSearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Projection?.FieldTransformations == null || query.Projection.FieldTransformations.Count == 0)
            {
                Logger.LogWarning("Transformation search called without field transformations for {EntityName}", EntityName);
                return ServiceResult<SearchResult<TransformationResult>>.Failure(
                    "Field transformations are required for transformation search. Use ExecuteSearchAsync for regular projections.");
            }

            var validationResult = await Validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                Logger.LogWarning("Transformation search query validation failed for {EntityName}: {Errors}",
                    EntityName, string.Join(", ", validationResult.Errors));
                return ServiceResult<SearchResult<TransformationResult>>.Failure(
                    "Search query validation failed", validationResult.Errors);
            }

            var countQuery = SqlQueryBuilder.BuildCountQuery(EntityMetadata, query);
            var selectQuery = SqlQueryBuilder.BuildSelectQuery(EntityMetadata, query);

            var totalCount = await DataExecutor.CountAsync(countQuery, cancellationToken);
            var entities = await DataExecutor.FetchAsync(selectQuery, cancellationToken);

            var transformationExpression = ExpressionBuilder.BuildTransformationExpression<TEntity>(
                query.Projection.FieldTransformations);
            var compiled = transformationExpression.Compile();

            var items = new List<TransformationResult>(entities.Count);
            foreach (var entity in entities)
                items.Add(compiled(entity));

            var result = new SearchResult<TransformationResult>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.Pagination.PageNumber,
                PageSize = query.Pagination.PageSize
            };

            Logger.LogInformation(
                "{EntityName} SQL transformation search executed: {TotalCount} total, page {Page} with {Count} items, {Transforms} transforms",
                EntityName, totalCount, query.Pagination.PageNumber, items.Count, query.Projection.FieldTransformations.Count);

            return ServiceResult<SearchResult<TransformationResult>>.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing SQL transformation search for {EntityName}", EntityName);
            return ServiceResult<SearchResult<TransformationResult>>.Failure($"Failed to execute transformation search: {ex.Message}");
        }
    }
}
