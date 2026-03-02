using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Onward.Base.AspNetCore.Authorization;

/// <summary>
/// Evaluates <see cref="OnwardPermissionRequirement"/> against the caller's claims.
/// <para>
/// Permission resolution order:
/// <list type="number">
///   <item>The <c>Admin</c> role bypasses all permission checks.</item>
///   <item>A role claim whose value equals <c>"{Resource}.{Action}"</c> (case-insensitive) grants access.</item>
///   <item>A <c>permissions</c> claim whose value equals <c>"{Resource}.{Action}"</c> grants access
///         (injected by <c>OnwardOnlineJwtBearerEventsHandler</c> during online introspection).</item>
/// </list>
/// </para>
/// </summary>
public sealed class OnwardPermissionAuthorizationHandler
    : AuthorizationHandler<OnwardPermissionRequirement>
{
    private const string AdminRole = "Admin";
    private const string PermissionsClaimType = "permissions";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OnwardPermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        // Admin role bypasses all permission checks
        if (context.User.IsInRole(AdminRole))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var permissionString = requirement.PermissionString;

        // Check role claims (ClaimsCurrentUserService convention: role = "Resource.Action")
        if (context.User.IsInRole(permissionString))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check explicit permission claims (injected by online introspection)
        var hasViaClaim = context.User.Claims.Any(c =>
            c.Type == PermissionsClaimType &&
            string.Equals(c.Value, permissionString, StringComparison.OrdinalIgnoreCase));

        if (hasViaClaim)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
