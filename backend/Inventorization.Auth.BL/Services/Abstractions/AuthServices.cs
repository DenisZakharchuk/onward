using Inventorization.Auth.BL.DataAccess.Repositories;
using Inventorization.Base.Abstractions;

namespace Inventorization.Auth.BL.Services.Abstractions;

/// <summary>
/// Interface for role and permission queries
/// </summary>
public interface IRolePermissionService
{
    Task<bool> UserHasPermissionAsync(
        Guid userId,
        string resource,
        string action,
        CancellationToken cancellationToken = default);

    Task<bool> UserHasAnyPermissionAsync(
        Guid userId,
        IEnumerable<string> permissionNames,
        CancellationToken cancellationToken = default);

    Task<bool> UserHasAllPermissionsAsync(
        Guid userId,
        IEnumerable<string> permissionNames,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for authorization checks
/// </summary>
public interface IAuthorizationService
{
    Task<bool> AuthorizeAsync(
        Guid userId,
        string resource,
        string action,
        CancellationToken cancellationToken = default);

    Task<AuthorizationContext?> GetAuthorizationContextAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Authorization context for a user
/// </summary>
public class AuthorizationContext
{
    public Guid UserId { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}
