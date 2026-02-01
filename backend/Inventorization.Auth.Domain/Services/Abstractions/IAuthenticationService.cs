using Inventorization.Auth.DTO.DTO.Auth;
using Inventorization.Base.DTOs;

namespace Inventorization.Auth.Domain.Services.Abstractions;

/// <summary>
/// Authentication service for login, token refresh, and logout operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service result with login response containing access token and refresh token</returns>
    Task<ServiceResult<LoginResponseDTO>> LoginAsync(
        string email,
        string password,
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token value</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service result with new access token and refresh token</returns>
    Task<ServiceResult<LoginResponseDTO>> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by revoking all active refresh tokens
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<bool>> LogoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
