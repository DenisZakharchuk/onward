using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Onward.Auth.BL.DbContexts;
using Onward.Auth.BL.Entities;
using Onward.Auth.BL.Services.Abstractions;

namespace Onward.Auth.BL.Services.Implementations;

/// <summary>
/// PostgreSQL-backed implementation of <see cref="ITokenBlacklist"/>.
/// Uses the <see cref="AuthDbContext"/> directly — this is infrastructure, not domain logic.
/// </summary>
public sealed class PostgresTokenBlacklist : ITokenBlacklist
{
    private readonly AuthDbContext _db;
    private readonly ILogger<PostgresTokenBlacklist> _logger;

    public PostgresTokenBlacklist(AuthDbContext db, ILogger<PostgresTokenBlacklist> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti)) return false;

        return await _db.BlacklistedTokens
            .AsNoTracking()
            .AnyAsync(t => t.Jti == jti && t.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task BlacklistAsync(
        string jti,
        DateTime expiresAt,
        string reason,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        // Idempotent — skip if already blacklisted
        var exists = await _db.BlacklistedTokens
            .AsNoTracking()
            .AnyAsync(t => t.Jti == jti, cancellationToken);

        if (exists)
        {
            _logger.LogDebug("JTI {Jti} is already blacklisted — skipping.", jti);
            return;
        }

        var entry = new BlacklistedToken(jti, expiresAt, reason, userId);
        _db.BlacklistedTokens.Add(entry);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Blacklisted JTI {Jti} for user {UserId}. Reason: {Reason}. Expires: {ExpiresAt:O}",
            jti, userId, reason, expiresAt);
    }

    public async Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expired = await _db.BlacklistedTokens
            .Where(t => t.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
            return 0;

        _db.BlacklistedTokens.RemoveRange(expired);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purged {Count} expired blacklist entries.", expired.Count);
        return expired.Count;
    }
}
