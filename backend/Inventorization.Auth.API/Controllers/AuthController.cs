using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Inventorization.Auth.Domain.Services.Abstractions;
using Inventorization.Auth.DTO.DTO.Auth;
using Inventorization.Base.DTOs;

namespace Inventorization.Auth.API.Controllers;

/// <summary>
/// Authentication endpoints (login, refresh token, logout)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT access token and refresh token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceResult<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Login attempt for email: {Email} from IP: {IpAddress}", request.Email, ipAddress);

        var result = await _authenticationService.LoginAsync(request.Email, request.Password, ipAddress);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New JWT access token and refresh token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceResult<LoginResponseDTO>>> RefreshToken([FromBody] RefreshTokenRequestDTO request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Token refresh attempt from IP: {IpAddress}", ipAddress);

        var result = await _authenticationService.RefreshTokenAsync(request.RefreshToken, ipAddress);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ServiceResult<bool>>> Logout()
    {
        // Extract user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Logout attempt with invalid user ID claim");
            return BadRequest(ServiceResult<bool>.Failure("Invalid user session"));
        }

        _logger.LogInformation("Logout attempt for user {UserId}", userId);

        var result = await _authenticationService.LogoutAsync(userId);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
