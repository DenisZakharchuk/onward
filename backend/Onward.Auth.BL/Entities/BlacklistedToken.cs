using Onward.Base.Models;

namespace Onward.Auth.BL.Entities;

/// <summary>
/// Records an access token (identified by its jti claim) that has been explicitly
/// revoked before its natural expiry.  Used by the online-auth introspection path
/// to reject already-invalidated tokens in real time.
/// </summary>
public sealed class BlacklistedToken : BaseEntity
{
    private BlacklistedToken() { }   // EF Core only

    /// <summary>Creates a new blacklist entry.</summary>
    public BlacklistedToken(
        string jti,
        DateTime expiresAt,
        string reason,
        Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException("JTI is required.", nameof(jti));
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Revocation reason is required.", nameof(reason));

        Id = Guid.NewGuid();
        Jti = jti;
        ExpiresAt = expiresAt;
        Reason = reason;
        UserId = userId;
        RevokedAt = DateTime.UtcNow;
    }

    /// <summary>The JWT unique identifier (jti claim) being blacklisted.</summary>
    public string Jti { get; private set; } = null!;

    /// <summary>When the original access token would have expired naturally.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>Human-readable reason for revocation.</summary>
    public string Reason { get; private set; } = null!;

    /// <summary>The user whose token was revoked (optional, for audit purposes).</summary>
    public Guid? UserId { get; private set; }

    /// <summary>Timestamp of revocation.</summary>
    public DateTime RevokedAt { get; private set; }
}
