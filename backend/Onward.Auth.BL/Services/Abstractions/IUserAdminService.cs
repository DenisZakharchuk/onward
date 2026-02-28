using Onward.Base.DTOs;

namespace Onward.Auth.BL.Services.Abstractions;

/// <summary>
/// Administrative operations for user account management (block / unblock).
/// Separate from <see cref="IAuthenticationService"/> to follow SRP.
/// </summary>
public interface IUserAdminService
{
    /// <summary>
    /// Deactivates a user account and revokes all their active refresh tokens.
    /// Any subsequently introspected access tokens for this user will return inactive.
    /// </summary>
    Task<ServiceResult<bool>> BlockUserAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>Reactivates a previously blocked user account.</summary>
    Task<ServiceResult<bool>> UnblockUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
