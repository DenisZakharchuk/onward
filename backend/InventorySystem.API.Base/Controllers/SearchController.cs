using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Inventorization.Base.DTOs;
using Inventorization.Base.Abstractions;

namespace InventorySystem.API.Base.Controllers;

/// <summary>
/// Abstract generic base controller for complex search, filtering, and pagination.
/// Extend this class for concrete entity search controllers.
/// </summary>
/// <typeparam name="TEntity">Domain model entity class</typeparam>
/// <typeparam name="TDetailsDTO">DTO for responses</typeparam>
/// <typeparam name="TSearchDTO">DTO for search/filtering operations</typeparam>
/// <typeparam name="TService">Data service implementation</typeparam>
public abstract class SearchController<TEntity, TDetailsDTO, TSearchDTO, TService>
    : ServiceController
    where TEntity : class
    where TDetailsDTO : class
    where TSearchDTO : class
    where TService : IDataService<TEntity, object, object, object, TDetailsDTO, TSearchDTO>
{
    protected readonly TService DataService;

    protected SearchController(TService dataService, ILogger<ServiceController> logger)
        : base(logger)
    {
        DataService = dataService;
    }

    /// <summary>
    /// Complex search with filtering, pagination, and sorting.
    /// HTTP POST /{controller}/search
    /// </summary>
    [HttpPost("search")]
    public virtual async Task<ActionResult<ServiceResult<PagedResult<TDetailsDTO>>>> SearchAsync(
        [FromBody] TSearchDTO searchDto,
        CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(SearchAsync), new { searchDto });

        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<PagedResult<TDetailsDTO>>.Failure("Invalid model state"));
        }

        try
        {
            var result = await DataService.SearchAsync(searchDto, cancellationToken);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(SearchAsync), new Exception(result.Message));
                return BadRequest(result);
            }

            LogOperationSuccess(nameof(SearchAsync));
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(SearchAsync), ex);
            return StatusCode(500, ServiceResult<PagedResult<TDetailsDTO>>.Failure($"Internal server error: {ex.Message}"));
        }
    }
}
