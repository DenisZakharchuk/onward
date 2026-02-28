namespace Onward.Auth.DTO.DTO.Auth;

/// <summary>Response DTO for the token introspection endpoint.</summary>
public sealed class IntrospectionResultDTO
{
    /// <summary>Whether the token is active and the user is allowed to proceed.</summary>
    public bool Active { get; set; }

    /// <summary>User ID associated with the token.</summary>
    public Guid UserId { get; set; }

    /// <summary>Email address of the token owner.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Current role names of the user (fresh from DB).</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>Current permission strings of the user (fresh from DB).</summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// <c>true</c> when the token is inactive because the user account is blocked.
    /// </summary>
    public bool Blocked { get; set; }

    /// <summary>Tenant context, if applicable.</summary>
    public string? TenantId { get; set; }

    /// <summary>Human-readable reason for the token being inactive.</summary>
    public string? InactiveReason { get; set; }

    // ── Factory helpers ──────────────────────────────────────────────────────

    public static IntrospectionResultDTO Inactive(string reason, bool blocked = false)
        => new() { Active = false, Blocked = blocked, InactiveReason = reason };
}
