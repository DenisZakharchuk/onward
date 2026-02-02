using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Inventorization.Base.DTOs;
using Inventorization.Base.Abstractions;

namespace InventorySystem.API.Base.Controllers;

/// <summary>
/// Abstract generic base controller providing standard CRUD operations for all entities.
/// Extend this class for concrete entity controllers.
/// </summary>
/// <typeparam name="TEntity">Domain model entity class</typeparam>
/// <typeparam name="TCreateDTO">DTO for create operations</typeparam>
/// <typeparam name="TUpdateDTO">DTO for update operations</typeparam>
/// <typeparam name="TDeleteDTO">DTO for delete operations</typeparam>
/// <typeparam name="TDetailsDTO">DTO for responses</typeparam>
/// <typeparam name="TSearchDTO">DTO for search/filtering operations</typeparam>
/// <typeparam name="TService">Data service implementation</typeparam>
public abstract class DataController<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO, TService>
    : ServiceController
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : class
    where TDeleteDTO : class
    where TDetailsDTO : BaseDTO
    where TSearchDTO : class
    where TService : IDataService<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>
{
    protected readonly TService DataService;

    protected DataController(TService dataService, ILogger<ServiceController> logger)
        : base(logger)
    {
        DataService = dataService;
    }

    /// <summary>
    /// Retrieve single entity by ID.
    /// HTTP GET /{controller}/{id}
    /// </summary>
    [HttpGet("{id}")]
    public virtual async Task<ActionResult<ServiceResult<TDetailsDTO>>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(GetByIdAsync), new { id });

        try
        {
            var result = await DataService.GetByIdAsync(id, cancellationToken);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(GetByIdAsync), new Exception(result.Message));
                return NotFound(result);
            }

            LogOperationSuccess(nameof(GetByIdAsync));
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(GetByIdAsync), ex);
            return StatusCode(500, ServiceResult<TDetailsDTO>.Failure($"Internal server error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Create new entity.
    /// HTTP POST /{controller}
    /// </summary>
    [HttpPost]
    public virtual async Task<ActionResult<ServiceResult<TDetailsDTO>>> CreateAsync(
        [FromBody] TCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(CreateAsync), new { dto });

        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<TDetailsDTO>.Failure("Invalid model state"));
        }

        try
        {
            var result = await DataService.AddAsync(dto, cancellationToken);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(CreateAsync), new Exception(result.Message));
                return BadRequest(result);
            }

            LogOperationSuccess(nameof(CreateAsync));
            // Return 201 Created with the result data
            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(CreateAsync), ex);
            return StatusCode(500, ServiceResult<TDetailsDTO>.Failure($"Internal server error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Update existing entity.
    /// HTTP PUT /{controller}/{id}
    /// </summary>
    [HttpPut("{id}")]
    public virtual async Task<ActionResult<ServiceResult<TDetailsDTO>>> UpdateAsync(
        Guid id,
        [FromBody] TUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(UpdateAsync), new { id, dto });

        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<TDetailsDTO>.Failure("Invalid model state"));
        }

        try
        {
            // Validate that the ID in the route matches the DTO
            var dtoIdProperty = dto.GetType().GetProperty("Id");
            if (dtoIdProperty != null)
            {
                var dtoId = (Guid?)dtoIdProperty.GetValue(dto);
                if (dtoId != id)
                {
                    return BadRequest(ServiceResult<TDetailsDTO>.Failure("ID mismatch between route and DTO"));
                }
            }

            var result = await DataService.UpdateAsync(dto, cancellationToken);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(UpdateAsync), new Exception(result.Message));
                return NotFound(result);
            }

            LogOperationSuccess(nameof(UpdateAsync));
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(UpdateAsync), ex);
            return StatusCode(500, ServiceResult<TDetailsDTO>.Failure($"Internal server error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Delete entity.
    /// HTTP DELETE /{controller}/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public virtual async Task<ActionResult<ServiceResult<bool>>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(DeleteAsync), new { id });

        try
        {
            // Create delete DTO with ID
            var deleteDto = (TDeleteDTO)Activator.CreateInstance(typeof(TDeleteDTO))!;
            var idProperty = typeof(TDeleteDTO).GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(deleteDto, id);
            }

            var result = await DataService.DeleteAsync(deleteDto, cancellationToken);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(DeleteAsync), new Exception(result.Message));
                return NotFound(result);
            }

            LogOperationSuccess(nameof(DeleteAsync));
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(DeleteAsync), ex);
            return StatusCode(500, ServiceResult<TDeleteDTO>.Failure($"Internal server error: {ex.Message}"));
        }
    }
}
