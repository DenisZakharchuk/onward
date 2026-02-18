using Inventorization.Base.Abstractions;
using Inventorization.Base.Ownership;

namespace Inventorization.Base.AspNetCore.Identity;

/// <summary>
/// Claims-based implementation of <see cref="ICurrentUserService{TOwnership}"/>.
/// Performs ownership and permission checks using the caller's
/// <see cref="ICurrentIdentityContext{TOwnership}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Permission convention: a caller is considered to have permission for
/// <c>"{resource}.{action}"</c> when their roles contain either the exact
/// combined permission string or a wildcard <c>Admin</c> role.
/// </para>
/// <para>
/// Ownership access: a caller may access an entity when:
/// <list type="bullet">
///   <item>Their ownership VO equals the entity's ownership VO (record structural equality), OR</item>
///   <item>They hold the <c>Admin</c> role.</item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="TOwnership">Concrete ownership VO for this bounded context.</typeparam>
public sealed class ClaimsCurrentUserService<TOwnership> : ICurrentUserService<TOwnership>
    where TOwnership : OwnershipValueObject
{
    private const string AdminRole = "Admin";

    private readonly ICurrentIdentityContext<TOwnership> _identityContext;

    public ClaimsCurrentUserService(ICurrentIdentityContext<TOwnership> identityContext)
    {
        _identityContext = identityContext ?? throw new ArgumentNullException(nameof(identityContext));
    }

    /// <inheritdoc />
    public Task<bool> HasPermissionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        if (!_identityContext.IsAuthenticated)
            return Task.FromResult(false);

        // Admin role bypasses all permission checks
        if (_identityContext.IsInRole(AdminRole))
            return Task.FromResult(true);

        // Expect a role claim with the exact permission string "resource.action"
        var permissionClaim = $"{resource}.{action}";
        var hasPermission = _identityContext.Roles.Contains(permissionClaim, StringComparer.OrdinalIgnoreCase);
        return Task.FromResult(hasPermission);
    }

    /// <inheritdoc />
    public Task<bool> CanAccessEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : IOwnedEntity<TOwnership>
    {
        if (!_identityContext.IsAuthenticated || _identityContext.Ownership is null)
            return Task.FromResult(false);

        // Admin role has cross-ownership access
        if (_identityContext.IsInRole(AdminRole))
            return Task.FromResult(true);

        // Record structural equality: all VO fields must match
        var canAccess = _identityContext.Ownership.Equals(entity.Ownership);
        return Task.FromResult(canAccess);
    }
}
