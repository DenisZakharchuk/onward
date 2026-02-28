using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onward.Auth.BL.Services.Abstractions;
using Onward.Auth.DTO.DTO.Auth;
using Onward.Base.DTOs;

namespace Onward.Auth.API.Controllers;

/// <summary>
/// Token management: introspection and explicit revocation.
/// The introspect endpoint is intentionally anonymous — it is called service-to-service
/// with a shared secret or network-level trust, not by end users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class TokensController : ControllerBase
{
    private readonly ITokenIntrospectionService _introspectionService;
    private readonly ITokenBlacklist _blacklist;
    private readonly ILogger<TokensController> _logger;

    public TokensController(
        ITokenIntrospectionService introspectionService,
        ITokenBlacklist blacklist,
        ILogger<TokensController> logger)
    {
        _introspectionService = introspectionService;
        _blacklist = blacklist;
        _logger = logger;
    }

    /// <summary>
    /// Validates an access token JTI and returns the current authorization context.
    /// Called by consumer microservices on every request in online-auth mode.
    /// </summary>
    /// <remarks>
    /// This endpoint is <c>[AllowAnonymous]</c> because the JWT signature has already been
    /// validated by the consumer service before it forwards the JTI here.
    /// Network-level trust (private VPC / service mesh) is assumed.
    /// </remarks>
    [HttpPost("introspect")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResult<IntrospectionResultDTO>), 200)]
    [ProducesResponseType(typeof(ServiceResult<IntrospectionResultDTO>), 400)]
    public async Task<ActionResult<ServiceResult<IntrospectionResultDTO>>> Introspect(
        [FromBody] IntrospectRequestDTO request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Jti))
            return BadRequest(ServiceResult<IntrospectionResultDTO>.Failure("UserId and Jti are required."));

        _logger.LogDebug("Introspection request: JTI={Jti} UserId={UserId}", request.Jti, request.UserId);

        var result = await _introspectionService.IntrospectAsync(
            request.Jti, request.UserId, request.TenantId, cancellationToken);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Explicitly revokes a single access token by its JTI.
    /// The token must be decoded client-side; the server only adds the JTI to the blacklist.
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceResult<bool>), 200)]
    [ProducesResponseType(typeof(ServiceResult<bool>), 400)]
    public async Task<ActionResult<ServiceResult<bool>>> RevokeToken(
        [FromBody] RevokeTokenDTO request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Jti))
            return BadRequest(ServiceResult<bool>.Failure("Jti is required."));

        // Parse the expiry from the caller's own JWT so we know how long to keep this blacklist entry.
        // If the caller's token cannot be inspected, default to a reasonable window.
        var callerJwt = HttpContext.User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        var expiresAt = callerJwt != null && long.TryParse(callerJwt, out var exp)
            ? DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime
            : DateTime.UtcNow.AddHours(1);

        var callerUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _ = Guid.TryParse(callerUserId, out var userId);

        await _blacklist.BlacklistAsync(
            request.Jti,
            expiresAt,
            request.Reason ?? "Explicit revocation",
            userId == Guid.Empty ? null : userId,
            cancellationToken);

        _logger.LogInformation("JTI {Jti} revoked by user {UserId}.", request.Jti, userId);
        return Ok(ServiceResult<bool>.Success(true, "Token revoked successfully."));
    }
}
