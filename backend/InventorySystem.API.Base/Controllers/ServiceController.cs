using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InventorySystem.API.Base.Controllers;

/// <summary>
/// Abstract non-generic base controller providing common functionality.
/// All service controllers should extend this class.
/// </summary>
public abstract class ServiceController : ControllerBase
{
    protected readonly ILogger<ServiceController> Logger;

    protected ServiceController(ILogger<ServiceController> logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Creates an OK response for successful operations.
    /// </summary>
    protected OkObjectResult OkResponse<T>(T data)
    {
        return Ok(data);
    }

    /// <summary>
    /// Creates a BadRequest response for validation/domain errors.
    /// </summary>
    protected BadRequestObjectResult BadRequestResponse<T>(T error)
    {
        return BadRequest(error);
    }

    /// <summary>
    /// Creates a NotFound response.
    /// </summary>
    protected NotFoundResult NotFoundResponse()
    {
        return NotFound();
    }

    /// <summary>
    /// Creates a CreatedAtAction response.
    /// </summary>
    protected CreatedAtActionResult CreatedResponse<T>(string actionName, string routeValueKey, object routeValue, T data)
    {
        return CreatedAtAction(actionName, new { id = routeValue }, data);
    }

    /// <summary>
    /// Logs operation start.
    /// </summary>
    protected void LogOperationStart(string operationName, object? parameters = null)
    {
        Logger.LogInformation("Operation started: {OperationName}", operationName);
    }

    /// <summary>
    /// Logs operation success.
    /// </summary>
    protected void LogOperationSuccess(string operationName)
    {
        Logger.LogInformation("Operation completed: {OperationName}", operationName);
    }

    /// <summary>
    /// Logs operation error.
    /// </summary>
    protected void LogOperationError(string operationName, Exception ex)
    {
        Logger.LogError(ex, "Operation failed: {OperationName}", operationName);
    }
}
