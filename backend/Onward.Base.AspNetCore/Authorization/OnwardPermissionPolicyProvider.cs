using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Onward.Base.AspNetCore.Authorization;

/// <summary>
/// Dynamic ASP.NET Core authorization policy provider that resolves
/// <c>"{Resource}.{Action}"</c> policy names (e.g. <c>"Product.Read"</c>) into
/// <see cref="AuthorizationPolicy"/> instances backed by <see cref="OnwardPermissionRequirement"/>.
/// <para>
/// Any policy name that does not match the <c>Resource.Action</c> pattern is delegated
/// to the <see cref="DefaultAuthorizationPolicyProvider"/>, so standard <c>[Authorize(Policy="…")]</c>
/// usages continue to work alongside <see cref="OnwardAuthorizeAttribute"/>.
/// </para>
/// </summary>
public sealed class OnwardPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    // Matches "Word.Word" — both segments must be non-empty identifiers
    private static readonly Regex PermissionPolicyPattern =
        new(@"^[A-Za-z]\w*\.[A-Za-z]\w*$", RegexOptions.Compiled);

    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public OnwardPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    /// <inheritdoc />
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallback.GetDefaultPolicyAsync();

    /// <inheritdoc />
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallback.GetFallbackPolicyAsync();

    /// <inheritdoc />
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (PermissionPolicyPattern.IsMatch(policyName))
        {
            var dot = policyName.IndexOf('.');
            var resource = policyName[..dot];
            var action   = policyName[(dot + 1)..];

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new OnwardPermissionRequirement(resource, action))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }
}
