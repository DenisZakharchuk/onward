using Microsoft.Extensions.Logging;
using Onward.Auth.BL.Services.Abstractions;
using Onward.Auth.DTO.DTO.Auth;
using Onward.Base.DTOs;

namespace Onward.Auth.BL.Services.Implementations;

/// <summary>
/// Validates an access token JTI by checking the blacklist, user active status,
/// and loading the current roles and permissions.
/// </summary>
public sealed class TokenIntrospectionService : ITokenIntrospectionService
{
    private readonly ITokenBlacklist _blacklist;
    private readonly IUserRepository _userRepository;
    private readonly IRolePermissionService _rolePermissionService;
    private readonly ILogger<TokenIntrospectionService> _logger;

    public TokenIntrospectionService(
        ITokenBlacklist blacklist,
        IUserRepository userRepository,
        IRolePermissionService rolePermissionService,
        ILogger<TokenIntrospectionService> logger)
    {
        _blacklist = blacklist;
        _userRepository = userRepository;
        _rolePermissionService = rolePermissionService;
        _logger = logger;
    }

    public async Task<ServiceResult<IntrospectionResultDTO>> IntrospectAsync(
        string jti,
        Guid userId,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Check the JTI blacklist first (fastest path to rejection)
            if (await _blacklist.IsBlacklistedAsync(jti, cancellationToken))
            {
                _logger.LogDebug("Introspection failed: JTI {Jti} is blacklisted.", jti);
                return ServiceResult<IntrospectionResultDTO>.Success(
                    IntrospectionResultDTO.Inactive("Token has been revoked."));
            }

            // 2. Verify the user exists and is active
            var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Introspection failed: user {UserId} not found.", userId);
                return ServiceResult<IntrospectionResultDTO>.Success(
                    IntrospectionResultDTO.Inactive("User not found."));
            }

            if (!user.IsActive)
            {
                _logger.LogInformation("Introspection failed: user {UserId} is blocked.", userId);
                return ServiceResult<IntrospectionResultDTO>.Success(
                    IntrospectionResultDTO.Inactive("User account is blocked.", blocked: true));
            }

            // 3. Load fresh roles and permissions
            var roles = (await _rolePermissionService.GetUserRolesAsync(userId, cancellationToken)).ToList();
            var permissions = (await _rolePermissionService.GetUserPermissionsAsync(userId, cancellationToken)).ToList();

            return ServiceResult<IntrospectionResultDTO>.Success(new IntrospectionResultDTO
            {
                Active = true,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles,
                Permissions = permissions,
                TenantId = tenantId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token introspection for JTI {Jti}, user {UserId}.", jti, userId);
            return ServiceResult<IntrospectionResultDTO>.Failure("An error occurred during token introspection.");
        }
    }
}
