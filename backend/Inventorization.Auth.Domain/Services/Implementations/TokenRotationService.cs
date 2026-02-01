using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inventorization.Auth.Domain.Services.Implementations;

/// <summary>
/// Token rotation service with reuse detection and family revocation
/// </summary>
public class TokenRotationService : ITokenRotationService
{
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IRefreshTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRolePermissionService _rolePermissionService;
    private readonly ILogger<TokenRotationService> _logger;

    public TokenRotationService(
        IJwtTokenProvider jwtTokenProvider,
        IRefreshTokenRepository tokenRepository,
        IUserRepository userRepository,
        IRolePermissionService rolePermissionService,
        ILogger<TokenRotationService> logger)
    {
        _jwtTokenProvider = jwtTokenProvider;
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _rolePermissionService = rolePermissionService;
        _logger = logger;
    }

    public async Task<(string accessToken, RefreshToken newRefreshToken)> RefreshTokenAsync(
        string refreshTokenValue,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        // Validate old token
        var oldToken = await ValidateRefreshTokenAsync(refreshTokenValue, cancellationToken);
        if (oldToken == null)
            throw new InvalidOperationException("Invalid or expired refresh token");

        if (oldToken.IsRevoked)
            throw new InvalidOperationException("Token has been revoked");

        // Check for token reuse attack
        if (oldToken.ReplacedByTokenId.HasValue)
        {
            _logger.LogWarning(
                "Token reuse detected for user {UserId}. Token family {Family} will be revoked.",
                oldToken.UserId,
                oldToken.Family);
            
            // Revoke entire family on reuse detection (security-first)
            await RevokeTokenFamilyAsync(oldToken.Family, "Token reuse detected", cancellationToken);
            throw new InvalidOperationException("Token has already been used. Family revoked due to potential security breach.");
        }

        // Get user and roles for new access token
        var user = await _userRepository.GetUserByIdAsync(oldToken.UserId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var roles = await _rolePermissionService.GetUserRolesAsync(oldToken.UserId, cancellationToken);
        var permissions = await _rolePermissionService.GetUserPermissionsAsync(oldToken.UserId, cancellationToken);

        // Generate new tokens
        var newAccessToken = _jwtTokenProvider.CreateAccessToken(user, roles, permissions);
        var newRefreshTokenValue = _jwtTokenProvider.CreateRefreshToken();

        // Create new refresh token entity using immutable constructor
        var newRefreshToken = new RefreshToken(
            userId: oldToken.UserId,
            token: newRefreshTokenValue,
            expiryDate: DateTime.UtcNow.AddDays(int.Parse("7")), // From config
            family: oldToken.Family, // Same family for chain tracking
            ipAddress: ipAddress
        );

        // Rotate old token using entity method
        oldToken.RotateToken(newRefreshToken.Id);

        await _tokenRepository.UpdateAsync(oldToken, cancellationToken);
        await _tokenRepository.CreateAsync(newRefreshToken, cancellationToken);

        _logger.LogInformation(
            "Token rotated successfully for user {UserId}. New token ID: {TokenId}, Rotation count: {RotationCount}",
            user.Id,
            newRefreshToken.Id,
            newRefreshToken.RotationCount);

        return (newAccessToken, newRefreshToken);
    }

    public async Task RevokeTokenAsync(
        Guid tokenId,
        string reason = "Manual revocation",
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenRepository.GetByIdAsync(tokenId, cancellationToken);
        if (token == null)
            throw new InvalidOperationException("Token not found");

        // Revoke token using entity method
        token.Revoke();

        await _tokenRepository.UpdateAsync(token, cancellationToken);

        _logger.LogInformation(
            "Token {TokenId} revoked. Reason: {Reason}",
            tokenId,
            reason);
    }

    public async Task RevokeTokenFamilyAsync(
        string family,
        string reason = "Reuse detected",
        CancellationToken cancellationToken = default)
    {
        var tokens = await _tokenRepository.GetTokensByFamilyAsync(family, cancellationToken);
        
        foreach (var token in tokens)
        {
            if (!token.IsRevoked)
            {
                // Revoke token using entity method
                token.Revoke();
                await _tokenRepository.UpdateAsync(token, cancellationToken);
            }
        }

        _logger.LogWarning(
            "Token family {Family} revoked. Reason: {Reason}. Tokens revoked: {TokenCount}",
            family,
            reason,
            tokens.Count());
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(
        string tokenValue,
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenRepository.GetByTokenValueAsync(tokenValue, cancellationToken);
        
        if (token == null)
        {
            _logger.LogWarning("Refresh token not found in database");
            return null;
        }

        if (token.IsRevoked)
        {
            _logger.LogWarning("Refresh token {TokenId} is revoked", token.Id);
            return null;
        }

        if (!token.IsValid)
        {
            _logger.LogWarning("Refresh token {TokenId} is expired", token.Id);
            return null;
        }

        return token;
    }
}
