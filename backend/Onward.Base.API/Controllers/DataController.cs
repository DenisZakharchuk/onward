using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Onward.Base.DTOs;
using Onward.Base.Abstractions;

namespace Onward.Base.API.Controllers;

/// <summary>
/// Abstract generic base controller providing standard CRUD operations for all entities.
/// Extend this class for concrete entity controllers.
/// </summary>
/// <typeparam name="TEntity">Domain model entity class</typeparam>
/// <typeparam name="TCreateDTO">DTO for create operations</typeparam>
/// <typeparam name="TUpdateDTO">DTO for update operations</typeparam>
/// <typeparam name="TDeleteDTO">DTO for delete operations</typeparam>
/// <typeparam name="TInitDTO">DTO for init operations</typeparam>
/// <typeparam name="TDetailsDTO">DTO for responses</typeparam>
/// <typeparam name="TSearchDTO">DTO for search/filtering operations</typeparam>
/// <typeparam name="TService">Data service implementation</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
[Produces("application/json")]
public abstract class DataController<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, TService, TKey>
    : ServiceController
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : BaseDTO<TKey>
    where TDeleteDTO : BaseDTO<TKey>
    where TInitDTO : InitDTO<TKey>
    where TDetailsDTO : BaseDTO<TKey>
    where TSearchDTO : class
    where TService : IDataService<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, TKey>
{
    protected readonly TService DataService;

    protected DataController(TService dataService, ILogger<ServiceController> logger)
        : base(logger)
    {
        DataService = dataService;
    }

    /// <summary>
    /// Initialize entity data using minimal init payload.
    /// HTTP POST /{controller}/init
    /// </summary>
    [HttpPost("init")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ServiceResult<>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ServiceResult<TDetailsDTO>>> InitAsync(
        [FromBody] TInitDTO dto,
        CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(InitAsync), new { dto });

        if (!ModelState.IsValid)
            return ModelValidationProblem();

        try
        {
            var result = await DataService.InitAsync(dto, cancellationToken);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(InitAsync), new Exception(result.Message));
                return NotFoundProblem(result.Message ?? "Resource not found for init");
            }

            LogOperationSuccess(nameof(InitAsync));
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(InitAsync), ex);
            return InternalErrorProblem($"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieve single entity by ID.
    /// HTTP GET /{controller}/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceResult<>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ServiceResult<TDetailsDTO>>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(GetByIdAsync), new { id });

        try
        {
            var result = await DataService.GetByIdAsync(id, cancellationToken);

            if (result.IsNotModified)
                return StatusCode(304);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(GetByIdAsync), new Exception(result.Message));
                return NotFoundProblem(result.Message ?? "Resource not found");
            }

            LogOperationSuccess(nameof(GetByIdAsync));
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(GetByIdAsync), ex);
            return InternalErrorProblem($"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Create new entity.
    /// HTTP POST /{controller}
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ServiceResult<>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ServiceResult<TDetailsDTO>>> CreateAsync(
        [FromBody] TCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(CreateAsync), new { dto });

        if (!ModelState.IsValid)
            return ModelValidationProblem();

        try
        {
            var result = await DataService.AddAsync(dto, cancellationToken);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(CreateAsync), new Exception(result.Message));
                if (result.IsConflict) return ConflictProblem(result.Message ?? "Resource already exists");
                return BadRequestProblem(result.Message ?? "Failed to create resource", result.Errors);
            }

            LogOperationSuccess(nameof(CreateAsync));
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(CreateAsync), ex);
            return InternalErrorProblem($"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update existing entity.
    /// HTTP PUT /{controller}/{id}
    /// </summary>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ServiceResult<>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ServiceResult<TDetailsDTO>>> UpdateAsync(
        TKey id,
        [FromBody] TUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(UpdateAsync), new { id, dto });

        if (!ModelState.IsValid)
            return ModelValidationProblem();

        try
        {
            // Validate that the ID in the route matches the DTO
            if (!EqualityComparer<TKey>.Default.Equals(dto.Id, id))
                return BadRequestProblem("ID mismatch between route and DTO");

            var result = await DataService.UpdateAsync(dto, cancellationToken);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(UpdateAsync), new Exception(result.Message));
                if (result.IsConflict) return ConflictProblem(result.Message ?? "Conflict — resource was modified");
                return NotFoundProblem(result.Message ?? "Resource not found");
            }

            LogOperationSuccess(nameof(UpdateAsync));
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(UpdateAsync), ex);
            return InternalErrorProblem($"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete entity.
    /// HTTP DELETE /{controller}/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ServiceResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<ServiceResult<bool>>> DeleteAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        LogOperationStart(nameof(DeleteAsync), new { id });

        try
        {
            // Create delete DTO with ID
            var deleteDto = (TDeleteDTO)Activator.CreateInstance(typeof(TDeleteDTO))!;
            deleteDto.Id = id;

            var result = await DataService.DeleteAsync(deleteDto, cancellationToken);

            if (!result.IsSuccess)
            {
                LogOperationError(nameof(DeleteAsync), new Exception(result.Message));
                if (result.IsConflict) return ConflictProblem(result.Message ?? "Conflict — resource cannot be deleted");
                return NotFoundProblem(result.Message ?? "Resource not found");
            }

            LogOperationSuccess(nameof(DeleteAsync));
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogOperationError(nameof(DeleteAsync), ex);
            return InternalErrorProblem($"Internal server error: {ex.Message}");
        }
    }
}

/// <summary>
/// Abstract generic base controller for Guid-primary-key entities — convenience alias.
/// </summary>
[Produces("application/json")]
public abstract class DataController<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, TService>
    : DataController<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, TService, Guid>
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : BaseDTO<Guid>
    where TDeleteDTO : BaseDTO<Guid>
    where TInitDTO : InitDTO<Guid>
    where TDetailsDTO : BaseDTO<Guid>
    where TSearchDTO : class
    where TService : IDataService<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO>
{
    protected DataController(TService dataService, ILogger<ServiceController> logger)
        : base(dataService, logger)
    {
    }
}
