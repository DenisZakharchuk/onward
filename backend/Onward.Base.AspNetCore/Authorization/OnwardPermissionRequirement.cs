using Microsoft.AspNetCore.Authorization;

namespace Onward.Base.AspNetCore.Authorization;

/// <summary>
/// ASP.NET Core authorization requirement that asserts the caller holds
/// a specific resource/action permission.
/// <para>
/// Paired with <see cref="OnwardPermissionAuthorizationHandler"/> and
/// <see cref="OnwardPermissionPolicyProvider"/> so that
/// <c>[OnwardAuthorize("Product", "Read")]</c> actually enforces access at the
/// ASP.NET Core authorization layer.
/// </para>
/// </summary>
/// <param name="Resource">The protected resource name (e.g. <c>"Product"</c>).</param>
/// <param name="Action">The guarded action name (e.g. <c>"Read"</c>).</param>
public sealed record OnwardPermissionRequirement(string Resource, string Action)
    : IAuthorizationRequirement
{
    /// <summary>
    /// The canonical permission string used in role/permission claims:
    /// <c>"{Resource}.{Action}"</c>.
    /// </summary>
    public string PermissionString => $"{Resource}.{Action}";
}
