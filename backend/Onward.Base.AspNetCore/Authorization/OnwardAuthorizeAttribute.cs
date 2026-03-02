using Microsoft.AspNetCore.Authorization;

namespace Onward.Base.AspNetCore.Authorization;

/// <summary>
/// Attribute for applying Onward role/permission-based authorization.
/// Apply at the controller class level; override at action level when needed.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class OnwardAuthorizeAttribute : AuthorizeAttribute
{
    /// <summary>Creates an <see cref="OnwardAuthorizeAttribute"/> requiring any authenticated user.</summary>
    public OnwardAuthorizeAttribute() { }

    /// <summary>
    /// Creates an <see cref="OnwardAuthorizeAttribute"/> requiring authorization for the specified resource (any action).
    /// The effective policy name is the resource name itself (e.g. <c>"Product"</c>).
    /// </summary>
    /// <param name="resource">The resource being protected (e.g. "Product").</param>
    public OnwardAuthorizeAttribute(string resource)
    {
        Resource = resource;
        Policy = resource;
    }

    /// <summary>
    /// Creates an <see cref="OnwardAuthorizeAttribute"/> requiring a specific permission.
    /// The effective policy name is <c>"resource.action"</c>.
    /// </summary>
    /// <param name="resource">The resource being protected (e.g. "Product").</param>
    /// <param name="action">The action being guarded (e.g. "Read", "Write", "Delete").</param>
    public OnwardAuthorizeAttribute(string resource, string action)
    {
        Resource = resource;
        Action = action;
        Policy = $"{resource}.{action}";
    }

    /// <summary>The resource component of the policy (e.g. "Product").</summary>
    public string? Resource { get; private set; }

    /// <summary>The action component of the policy (e.g. "Read").</summary>
    public string? Action { get; private set; }
}
