using Inventorization.Auth.BL.Entities;

namespace Inventorization.Auth.BL.Services.Abstractions;

/// <summary>
/// Token rotation service with reuse detection
/// </summary>
public interface ITokenRotationService
{
    /// <summary>
    /// Refreshes a token by creating a new access token and refresh token
    /// Detects and prevents token reuse attacks
    /// </summary>
    Task<(string accessToken, RefreshToken newRefreshToken)> RefreshTokenAsync(
        string refreshTokenValue,
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a token and optionally its entire family
    /// </summary>
    Task RevokeTokenAsync(
        Guid tokenId,
        string reason = "Manual revocation",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all tokens in a family
    /// </summary>
    Task RevokeTokenFamilyAsync(
        string family,
        string reason = "Reuse detected",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a refresh token
    /// </summary>
    Task<RefreshToken?> ValidateRefreshTokenAsync(
        string tokenValue,
        CancellationToken cancellationToken = default);
}
