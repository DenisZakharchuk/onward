using Inventorization.Base.Models;

namespace Inventorization.Auth.Domain.Entities;

/// <summary>
/// Represents a refresh token with rotation and reuse detection support
/// </summary>
public class RefreshToken : BaseEntity
{
    private RefreshToken() { }  // EF Core only

    /// <summary>
    /// Creates a new refresh token for a user
    /// </summary>
    public RefreshToken(Guid userId, string token, DateTime expiryDate, string family, string ipAddress, string? userAgent = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID is required", nameof(userId));
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token is required", nameof(token));
        if (expiryDate <= DateTime.UtcNow) throw new ArgumentException("Expiry date must be in the future", nameof(expiryDate));
        if (string.IsNullOrWhiteSpace(family)) throw new ArgumentException("Token family is required", nameof(family));
        if (string.IsNullOrWhiteSpace(ipAddress)) throw new ArgumentException("IP address is required", nameof(ipAddress));
        
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiryDate = expiryDate;
        Family = family;
        RotationCount = 0;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiryDate { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }  // Links to new token on rotation
    public string Family { get; private set; } = null!;  // Groups rotation chain for reuse detection
    public int RotationCount { get; private set; }
    public string IpAddress { get; private set; } = null!;
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public User User { get; } = null!;
    public RefreshToken? ReplacedByToken { get; private set; }

    /// <summary>
    /// Checks if token is valid (not expired and not revoked)
    /// </summary>
    public bool IsValid => DateTime.UtcNow < ExpiryDate && RevokedAt == null;

    /// <summary>
    /// Checks if token has been revoked
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Rotates the token by marking it as replaced and incrementing rotation count
    /// </summary>
    public void RotateToken(Guid newTokenId)
    {
        if (newTokenId == Guid.Empty) throw new ArgumentException("New token ID is required", nameof(newTokenId));
        if (RevokedAt.HasValue) throw new InvalidOperationException("Cannot rotate a revoked token");
        
        ReplacedByTokenId = newTokenId;
        RotationCount++;
    }

    /// <summary>
    /// Revokes the token
    /// </summary>
    public void Revoke()
    {
        if (RevokedAt.HasValue) throw new InvalidOperationException("Token is already revoked");
        
        RevokedAt = DateTime.UtcNow;
    }
}
