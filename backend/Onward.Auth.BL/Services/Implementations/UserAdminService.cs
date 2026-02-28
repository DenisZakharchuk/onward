using Microsoft.Extensions.Logging;
using Onward.Auth.BL.DataAccess.UnitOfWork;
using Onward.Auth.BL.Services.Abstractions;
using Onward.Base.DTOs;
using IUserRepo         = Onward.Auth.BL.Services.Abstractions.IUserRepository;
using IRefreshTokenRepo = Onward.Auth.BL.Services.Abstractions.IRefreshTokenRepository;

namespace Onward.Auth.BL.Services.Implementations;

/// <summary>
/// Handles administrative user state changes: block and unblock.
/// Blocking deactivates the account and revokes all active refresh tokens.
/// The blocking is enforced at introspection time — JTI blacklisting is not required
/// because <see cref="ITokenIntrospectionService"/> checks <c>IsActive</c> directly.
/// </summary>
public sealed class UserAdminService : IUserAdminService
{
    // Use the Services.Abstractions version of IUserRepository
    private readonly IUserRepo _userRepository;
    private readonly IRefreshTokenRepo _tokenRepository;
    private readonly IAuthUnitOfWork _unitOfWork;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(
        IUserRepo userRepository,
        IRefreshTokenRepo tokenRepository,
        IAuthUnitOfWork unitOfWork,
        ILogger<UserAdminService> logger)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> BlockUserAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user is null)
                return ServiceResult<bool>.Failure($"User {userId} not found.");

            if (!user.IsActive)
            {
                _logger.LogDebug("User {UserId} is already blocked.", userId);
                return ServiceResult<bool>.Success(true, "User is already blocked.");
            }

            // Deactivate account
            user.Deactivate();
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Revoke all active refresh tokens so existing sessions cannot be refreshed
            var activeTokens = await _tokenRepository.GetActiveTokensByUserIdAsync(userId, cancellationToken);
            foreach (var token in activeTokens)
                token.Revoke();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} blocked. Reason: {Reason}", userId, reason);
            return ServiceResult<bool>.Success(true, "User blocked successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking user {UserId}.", userId);
            return ServiceResult<bool>.Failure("An error occurred while blocking the user.");
        }
    }

    public async Task<ServiceResult<bool>> UnblockUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user is null)
                return ServiceResult<bool>.Failure($"User {userId} not found.");

            if (user.IsActive)
                return ServiceResult<bool>.Success(true, "User is already active.");

            user.Activate();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} unblocked.", userId);
            return ServiceResult<bool>.Success(true, "User unblocked successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking user {UserId}.", userId);
            return ServiceResult<bool>.Failure("An error occurred while unblocking the user.");
        }
    }
}
