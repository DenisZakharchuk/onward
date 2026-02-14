using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Inventorization.Base.Abstractions;
using Inventorization.Base.ADTs;
using Inventorization.Base.DTOs;
using Inventorization.Base.Models;
using Microsoft.Extensions.Logging;

namespace InventorySystem.API.Base.Controllers;

/// <summary>
/// Base controller for executing flexible ADT-based queries on entities.
/// Provides endpoints for both regular projections and field transformations.
/// Derived controllers specify entity type, projection type, and route.
/// </summary>
/// <typeparam name="TEntity">The entity type to query</typeparam>
/// <typeparam name="TProjection">The projection result type</typeparam>
public abstract class BaseQueryController<TEntity, TProjection> : ControllerBase
    where TEntity : class
    where TProjection : class, new()
{
    protected readonly ISearchService<TEntity, TProjection> SearchService;
    protected readonly ILogger Logger;
    
    protected BaseQueryController(
        ISearchService<TEntity, TProjection> searchService,
        ILogger logger)
    {
        SearchService = searchService;
        Logger = logger;
    }
    
    /// <summary>
    /// Gets the entity type name for logging purposes
    /// </summary>
    protected virtual string EntityName => typeof(TEntity).Name;
    
    /// <summary>
    /// Execute a flexible ADT-based query
    /// Supports complex filtering, projection, sorting, and pagination
    /// </summary>
    /// <param name="query">Search query ADT containing filter, projection, sort, and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated search results with requested projections</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SearchResult<>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ServiceResult<SearchResult<TProjection>>>> Query(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation("Executing ADT-based query on {EntityName}", EntityName);
        
        var result = await SearchService.ExecuteSearchAsync(query, cancellationToken);
        
        if (!result.IsSuccess)
        {
            Logger.LogWarning("Query execution failed for {EntityName}: {Errors}", 
                EntityName, string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Execute a query with field transformations
    /// Returns computed fields with custom aliases using transformations like:
    /// - String operations (upper, lower, substring, concat)
    /// - Arithmetic operations (add, subtract, multiply, divide)
    /// - Comparisons and conditionals
    /// - Type casts and coalesce
    /// </summary>
    /// <param name="query">Search query with field transformations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated results with transformation-computed fields</returns>
    [HttpPost("transform")]
    [ProducesResponseType(typeof(SearchResult<TransformationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ServiceResult<SearchResult<TransformationResult>>>> QueryWithTransformations(
        [FromBody] SearchQuery query,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation("Executing transformation query on {EntityName}", EntityName);
        
        var result = await SearchService.ExecuteTransformationSearchAsync(query, cancellationToken);
        
        if (!result.IsSuccess)
        {
            Logger.LogWarning("Transformation query execution failed for {EntityName}: {Errors}", 
                EntityName, string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}
