using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Onward.Base.API.Controllers;

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

    /// <summary>Returns a 404 RFC 9457 ProblemDetails response.</summary>
    protected ObjectResult NotFoundProblem(string detail, string? instance = null)
    {
        var pd = new ProblemDetails
        {
            Detail = detail,
            Instance = instance ?? HttpContext?.Request.Path.Value,
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found"
        };
        return new ObjectResult(pd) { StatusCode = StatusCodes.Status404NotFound };
    }

    /// <summary>Returns a 409 RFC 9457 ProblemDetails response.</summary>
    protected ObjectResult ConflictProblem(string detail, string? instance = null)
    {
        var pd = new ProblemDetails
        {
            Detail = detail,
            Instance = instance ?? HttpContext?.Request.Path.Value,
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict"
        };
        return new ObjectResult(pd) { StatusCode = StatusCodes.Status409Conflict };
    }

    /// <summary>
    /// Returns a 400 RFC 9457 response.
    /// When <paramref name="errors"/> is non-empty returns a ValidationProblemDetails with
    /// per-field errors; otherwise returns a plain ProblemDetails.
    /// </summary>
    protected ObjectResult BadRequestProblem(string detail, List<string>? errors = null)
    {
        if (errors is { Count: > 0 })
        {
            var vpd = new ValidationProblemDetails
            {
                Detail = detail,
                Instance = HttpContext?.Request.Path.Value,
                Status = StatusCodes.Status400BadRequest
            };
            vpd.Errors["errors"] = errors.ToArray();
            return new ObjectResult(vpd) { StatusCode = StatusCodes.Status400BadRequest };
        }
        var pd = new ProblemDetails
        {
            Detail = detail,
            Instance = HttpContext?.Request.Path.Value,
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request"
        };
        return new BadRequestObjectResult(pd);
    }

    /// <summary>Returns a 400 ValidationProblemDetails response built from the current ModelState.</summary>
    protected ObjectResult ModelValidationProblem()
    {
        var vpd = new ValidationProblemDetails(ModelState)
        {
            Instance = HttpContext?.Request.Path.Value,
            Status = StatusCodes.Status400BadRequest
        };
        return new ObjectResult(vpd) { StatusCode = StatusCodes.Status400BadRequest };
    }

    /// <summary>Returns a 500 RFC 9457 ProblemDetails response.</summary>
    protected ObjectResult InternalErrorProblem(string detail)
    {
        var pd = new ProblemDetails
        {
            Detail = detail,
            Instance = HttpContext?.Request.Path.Value,
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error"
        };
        return new ObjectResult(pd) { StatusCode = StatusCodes.Status500InternalServerError };
    }
}
