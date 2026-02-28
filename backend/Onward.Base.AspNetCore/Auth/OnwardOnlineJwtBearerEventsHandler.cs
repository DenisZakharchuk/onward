using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Onward.Base.Auth;

namespace Onward.Base.AspNetCore.Auth;

/// <summary>
/// JWT bearer events handler that performs online introspection after the JWT signature
/// and lifetime have been validated by the standard ASP.NET Core middleware.
/// </summary>
public sealed class OnwardOnlineJwtBearerEventsHandler
{
    private readonly IAuthIntrospectionClient _client;
    private readonly OnwardOnlineAuthSettings _settings;
    private readonly ILogger<OnwardOnlineJwtBearerEventsHandler> _logger;

    public OnwardOnlineJwtBearerEventsHandler(
        IAuthIntrospectionClient client,
        IOptions<OnwardOnlineAuthSettings> options,
        ILogger<OnwardOnlineJwtBearerEventsHandler> logger)
    {
        _client = client;
        _settings = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Invoked after the JWT signature and lifetime are validated successfully.
    /// Calls the Auth Service to verify the token is not revoked and the user is not blocked.
    /// </summary>
    public async Task OnTokenValidated(TokenValidatedContext context)
    {
        var principal = context.Principal;
        if (principal is null)
        {
            context.Fail("No principal present after JWT validation.");
            return;
        }

        var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrWhiteSpace(jti))
        {
            context.Fail("Token is missing the required 'jti' claim.");
            return;
        }

        var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = principal.FindFirstValue("tenant_id");

        try
        {
            var result = await _client.IntrospectAsync(jti, tenantId, context.HttpContext.RequestAborted);

            if (!result.Active)
            {
                var reason = result.Blocked
                    ? "User account is blocked."
                    : result.InactiveReason ?? "Token has been revoked.";

                _logger.LogInformation(
                    "Online auth rejected JTI {Jti} for user {UserId}: {Reason}",
                    jti, userIdStr, reason);

                context.Fail(reason);
                return;
            }

            // Optionally enrich the identity with fresh roles/permissions from introspection.
            // This ensures stale claims in the JWT are overridden with current DB state.
            if (result.Roles.Count > 0 || result.Permissions.Count > 0)
            {
                var freshClaims = new List<Claim>();

                foreach (var role in result.Roles)
                    freshClaims.Add(new Claim(ClaimTypes.Role, role));

                if (result.Permissions.Count > 0)
                    freshClaims.Add(new Claim("permissions", string.Join(",", result.Permissions)));

                var freshIdentity = new ClaimsIdentity(freshClaims);
                principal.AddIdentity(freshIdentity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Auth Service for JTI {Jti}.", jti);

            if (_settings.FailOpen)
            {
                _logger.LogWarning(
                    "FailOpen=true: allowing request through despite Auth Service error for JTI {Jti}.", jti);
                // Allow without modification
            }
            else
            {
                context.Fail("Auth Service is unavailable. Request rejected (FailOpen=false).");
            }
        }
    }
}
