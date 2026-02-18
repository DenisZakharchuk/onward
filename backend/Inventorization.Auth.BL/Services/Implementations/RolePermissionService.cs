using Inventorization.Auth.BL.Entities;
using Inventorization.Auth.BL.Services.Abstractions;
using Inventorization.Auth.BL.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventorization.Auth.BL.Services.Implementations;

/// <summary>
/// Role permission service for querying user permissions
/// </summary>
public class RolePermissionService : IRolePermissionService
{
    private readonly AuthDbContext _dbContext;
    private readonly ILogger<RolePermissionService> _logger;

    public RolePermissionService(AuthDbContext dbContext, ILogger<RolePermissionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> UserHasPermissionAsync(
        Guid userId,
        string resource,
        string action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var hasPermission = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission)
                .AnyAsync(
                    p => p.Resource == resource && p.Action == action,
                    cancellationToken);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Resource}.{Action} for user {UserId}", resource, action, userId);
            return false;
        }
    }

    public async Task<bool> UserHasAnyPermissionAsync(
        Guid userId,
        IEnumerable<string> permissionNames,
        CancellationToken cancellationToken = default)
    {
        var permList = permissionNames.ToList();
        if (!permList.Any())
            return true;

        try
        {
            var hasAny = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .AnyAsync(name => permList.Contains(name), cancellationToken);

            return hasAny;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking any permissions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UserHasAllPermissionsAsync(
        Guid userId,
        IEnumerable<string> permissionNames,
        CancellationToken cancellationToken = default)
    {
        var permList = permissionNames.ToList();
        if (!permList.Any())
            return true;

        try
        {
            var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
            var userPermSet = new HashSet<string>(userPermissions);

            return permList.All(perm => userPermSet.Contains(perm));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking all permissions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var permissions = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync(cancellationToken);

            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync(cancellationToken);

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }
}
