using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.Domain.Services.Abstractions;
using Inventorization.Auth.Domain.DataAccess.UnitOfWork;
using Inventorization.Auth.DTO.DTO.Auth;
using Inventorization.Base.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IPasswordHasher = Inventorization.Base.Abstractions.IPasswordHasher;

namespace Inventorization.Auth.Domain.Services.Implementations;

/// <summary>
/// Authentication service implementation handling login, token refresh, and logout
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _tokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly ITokenRotationService _tokenRotationService;
    private readonly IRolePermissionService _rolePermissionService;
    private readonly IAuthUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository tokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenProvider jwtTokenProvider,
        ITokenRotationService tokenRotationService,
        IRolePermissionService rolePermissionService,
        IAuthUnitOfWork unitOfWork,
        IConfiguration config,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenProvider = jwtTokenProvider;
        _tokenRotationService = tokenRotationService;
        _rolePermissionService = rolePermissionService;
        _unitOfWork = unitOfWork;
        _config = config;
        _logger = logger;
    }

    public async Task<ServiceResult<LoginResponseDTO>> LoginAsync(
        string email,
        string password,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Login attempt with empty email or password");
                return ServiceResult<LoginResponseDTO>.Failure("Email and password are required");
            }

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", email);
                return ServiceResult<LoginResponseDTO>.Failure("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {Email}", email);
                return ServiceResult<LoginResponseDTO>.Failure("User account is inactive");
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", email);
                return ServiceResult<LoginResponseDTO>.Failure("Invalid email or password");
            }

            // Get user roles and permissions
            var roles = await _rolePermissionService.GetUserRolesAsync(user.Id, cancellationToken);
            var permissions = await _rolePermissionService.GetUserPermissionsAsync(user.Id, cancellationToken);

            // Generate tokens
            var accessToken = _jwtTokenProvider.CreateAccessToken(user, roles, permissions);
            var refreshTokenValue = _jwtTokenProvider.CreateRefreshToken();

            // Create refresh token entity
            var jwtSettings = _config.GetSection("JwtSettings");
            var refreshTokenExpiryDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");
            
            var refreshToken = new RefreshToken(
                userId: user.Id,
                token: refreshTokenValue,
                expiryDate: DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                family: Guid.NewGuid().ToString(), // New family for this login session
                ipAddress: ipAddress
            );

            // Save refresh token
            await _tokenRepository.CreateAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {Email} logged in successfully", email);

            return ServiceResult<LoginResponseDTO>.Success(
                new LoginResponseDTO
                {
                    UserId = user.Id,
                    Email = user.Email,
                    AccessToken = accessToken,
                    RefreshToken = refreshTokenValue,
                    ExpiresIn = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15") * 60 // Convert to seconds
                },
                "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", email);
            return ServiceResult<LoginResponseDTO>.Failure("An error occurred during login");
        }
    }

    public async Task<ServiceResult<LoginResponseDTO>> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("Refresh token attempt with empty token");
                return ServiceResult<LoginResponseDTO>.Failure("Refresh token is required");
            }

            // Use token rotation service to refresh
            var (newAccessToken, newRefreshToken) = await _tokenRotationService.RefreshTokenAsync(
                refreshToken,
                ipAddress,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var jwtSettings = _config.GetSection("JwtSettings");
            _logger.LogInformation("Token refreshed successfully for user {UserId}", newRefreshToken.UserId);

            return ServiceResult<LoginResponseDTO>.Success(
                new LoginResponseDTO
                {
                    UserId = newRefreshToken.UserId,
                    Email = "", // Not available in refresh token flow
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    ExpiresIn = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15") * 60
                },
                "Token refreshed successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid refresh token attempt");
            return ServiceResult<LoginResponseDTO>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return ServiceResult<LoginResponseDTO>.Failure("An error occurred during token refresh");
        }
    }

    public async Task<ServiceResult<bool>> LogoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Logout attempt with empty user ID");
                return ServiceResult<bool>.Failure("Invalid user ID");
            }

            // Get all active tokens for user
            var activeTokens = await _tokenRepository.GetActiveTokensByUserIdAsync(userId, cancellationToken);

            // Revoke all active tokens
            foreach (var token in activeTokens)
            {
                token.Revoke();
                await _tokenRepository.UpdateAsync(token, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} logged out successfully. Revoked {TokenCount} tokens", userId, activeTokens.Count());

            return ServiceResult<bool>.Success(true, "Logout successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return ServiceResult<bool>.Failure("An error occurred during logout");
        }
    }
}
