namespace Onward.Auth.BL.Services.Abstractions;

/// <summary>
/// Manages the set of blacklisted access-token JTIs backed by PostgreSQL.
/// Swap implementations (Redis, in-memory, etc.) by registering a different
/// concrete class against this interface in DI.
/// </summary>
public interface ITokenBlacklist
{
    /// <summary>Returns <c>true</c> if <paramref name="jti"/> has been explicitly revoked.</summary>
    Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds <paramref name="jti"/> to the blacklist until <paramref name="expiresAt"/>.
    /// Idempotent — calling again for the same JTI is a no-op.
    /// </summary>
    Task BlacklistAsync(
        string jti,
        DateTime expiresAt,
        string reason,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all blacklist entries whose <c>ExpiresAt</c> is in the past.
    /// Call this from a background job or health-check endpoint to keep the table small.
    /// </summary>
    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default);
}
