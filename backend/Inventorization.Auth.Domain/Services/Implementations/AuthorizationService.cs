using Inventorization.Auth.Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inventorization.Auth.Domain.Services.Implementations;

/// <summary>
/// Authorization service for permission checks
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IRolePermissionService _rolePermissionService;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IRolePermissionService rolePermissionService,
        ILogger<AuthorizationService> logger)
    {
        _rolePermissionService = rolePermissionService;
        _logger = logger;
    }

    public async Task<bool> AuthorizeAsync(
        Guid userId,
        string resource,
        string action,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Authorization check with empty user ID");
            return false;
        }

        var hasPermission = await _rolePermissionService.UserHasPermissionAsync(
            userId,
            resource,
            action,
            cancellationToken);

        if (!hasPermission)
        {
            _logger.LogWarning(
                "Authorization denied for user {UserId}: Missing permission {Resource}.{Action}",
                userId,
                resource,
                action);
        }

        return hasPermission;
    }

    public async Task<AuthorizationContext?> GetAuthorizationContextAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _rolePermissionService.GetUserRolesAsync(userId, cancellationToken);
            var permissions = await _rolePermissionService.GetUserPermissionsAsync(userId, cancellationToken);

            return new AuthorizationContext
            {
                UserId = userId,
                Roles = roles,
                Permissions = permissions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authorization context for user {UserId}", userId);
            return null;
        }
    }
}
