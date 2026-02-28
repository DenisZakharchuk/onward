namespace Onward.Base.Auth;

/// <summary>
/// Result of a token introspection call to the Auth Service.
/// </summary>
public sealed class IntrospectionResult
{
    /// <summary>Whether the token is active (not revoked, user not blocked, not expired).</summary>
    public bool Active { get; init; }

    /// <summary>The user ID embedded in the token.</summary>
    public Guid UserId { get; init; }

    /// <summary>The email address of the token owner.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Role names assigned to the user at the time of introspection.</summary>
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();

    /// <summary>Permission strings (resource.action) assigned to the user.</summary>
    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();

    /// <summary>
    /// <c>true</c> if the token is inactive because the user has been explicitly blocked.
    /// </summary>
    public bool Blocked { get; init; }

    /// <summary>Tenant identifier (if multi-tenant mode is active).</summary>
    public string? TenantId { get; init; }

    /// <summary>Human-readable reason the token was rejected, if <see cref="Active"/> is <c>false</c>.</summary>
    public string? InactiveReason { get; init; }

    public static IntrospectionResult ActiveResult(
        Guid userId,
        string email,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions,
        string? tenantId = null)
        => new()
        {
            Active = true,
            UserId = userId,
            Email = email,
            Roles = roles,
            Permissions = permissions,
            TenantId = tenantId
        };

    public static IntrospectionResult InactiveResult(string reason, bool blocked = false)
        => new() { Active = false, Blocked = blocked, InactiveReason = reason };
}
