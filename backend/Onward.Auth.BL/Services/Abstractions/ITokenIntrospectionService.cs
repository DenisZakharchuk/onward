using Onward.Auth.DTO.DTO.Auth;
using Onward.Base.DTOs;

namespace Onward.Auth.BL.Services.Abstractions;

/// <summary>
/// Server-side token introspection service.
/// Validates a JTI and returns fresh authorization context for the online-auth path.
/// </summary>
public interface ITokenIntrospectionService
{
    /// <summary>
    /// Introspects a token by its JTI claim.
    /// Checks the blacklist, user active status, and loads current roles/permissions.
    /// </summary>
    /// <param name="jti">The JWT unique identifier claim value.</param>
    /// <param name="userId">The user ID embedded in the token (caller extracted from JWT).</param>
    /// <param name="tenantId">Optional tenant context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ServiceResult<IntrospectionResultDTO>> IntrospectAsync(
        string jti,
        Guid userId,
        string? tenantId = null,
        CancellationToken cancellationToken = default);
}
