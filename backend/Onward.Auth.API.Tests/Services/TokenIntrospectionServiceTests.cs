using Moq;
using Microsoft.Extensions.Logging;
using Onward.Auth.BL.Entities;
using Onward.Auth.BL.Services.Abstractions;
using Onward.Auth.BL.Services.Implementations;
using Onward.Auth.DTO.DTO.Auth;

namespace Onward.Auth.API.Tests.Services;

public class TokenIntrospectionServiceTests
{
    private readonly Mock<ITokenBlacklist> _blacklist = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRolePermissionService> _rolePermissionService = new();
    private readonly Mock<ILogger<TokenIntrospectionService>> _logger = new();

    private TokenIntrospectionService CreateSut() =>
        new(_blacklist.Object, _userRepo.Object, _rolePermissionService.Object, _logger.Object);

    // ── Happy path ──────────────────────────────────────────────────────────

    [Fact]
    public async Task IntrospectAsync_ActiveUser_ReturnsActiveResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = BuildActiveUser(userId);

        _blacklist.Setup(b => b.IsBlacklistedAsync("jti1", default)).ReturnsAsync(false);
        _userRepo.Setup(r => r.GetUserByIdAsync(userId, default)).ReturnsAsync(user);
        _rolePermissionService.Setup(r => r.GetUserRolesAsync(userId, default))
            .ReturnsAsync(new[] { "Admin" });
        _rolePermissionService.Setup(r => r.GetUserPermissionsAsync(userId, default))
            .ReturnsAsync(new[] { "products.create" });

        // Act
        var result = await CreateSut().IntrospectAsync("jti1", userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.Active);
        Assert.Contains("Admin", result.Data.Roles);
        Assert.Contains("products.create", result.Data.Permissions);
    }

    // ── Blacklisted JTI ────────────────────────────────────────────────────

    [Fact]
    public async Task IntrospectAsync_BlacklistedJti_ReturnsInactive()
    {
        var userId = Guid.NewGuid();
        _blacklist.Setup(b => b.IsBlacklistedAsync("revoked-jti", default)).ReturnsAsync(true);

        var result = await CreateSut().IntrospectAsync("revoked-jti", userId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Data!.Active);
        Assert.False(result.Data.Blocked);

        // User repo should NOT be called — we short-circuit on blacklist hit
        _userRepo.Verify(r => r.GetUserByIdAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    // ── User not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task IntrospectAsync_UserNotFound_ReturnsInactive()
    {
        var userId = Guid.NewGuid();
        _blacklist.Setup(b => b.IsBlacklistedAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _userRepo.Setup(r => r.GetUserByIdAsync(userId, default)).ReturnsAsync((User?)null);

        var result = await CreateSut().IntrospectAsync("jti", userId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Data!.Active);
    }

    // ── Blocked user ───────────────────────────────────────────────────────

    [Fact]
    public async Task IntrospectAsync_InactiveUser_ReturnsBlockedFlag()
    {
        var userId = Guid.NewGuid();
        var user = BuildBlockedUser(userId);

        _blacklist.Setup(b => b.IsBlacklistedAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _userRepo.Setup(r => r.GetUserByIdAsync(userId, default)).ReturnsAsync(user);

        var result = await CreateSut().IntrospectAsync("jti", userId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Data!.Active);
        Assert.True(result.Data.Blocked);
    }

    // ── Tenant context is forwarded ────────────────────────────────────────

    [Fact]
    public async Task IntrospectAsync_WithTenantId_PropagatesItToResult()
    {
        var userId = Guid.NewGuid();
        var user = BuildActiveUser(userId);
        const string tenantId = "tenant-abc";

        _blacklist.Setup(b => b.IsBlacklistedAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _userRepo.Setup(r => r.GetUserByIdAsync(userId, default)).ReturnsAsync(user);
        _rolePermissionService.Setup(r => r.GetUserRolesAsync(userId, default)).ReturnsAsync(Array.Empty<string>());
        _rolePermissionService.Setup(r => r.GetUserPermissionsAsync(userId, default)).ReturnsAsync(Array.Empty<string>());

        var result = await CreateSut().IntrospectAsync("jti", userId, tenantId);

        Assert.Equal(tenantId, result.Data!.TenantId);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static User BuildActiveUser(Guid id)
    {
        var user = new User(id.ToString() + "@test.com", "hashedPw", "Test User");
        return user;
    }

    private static User BuildBlockedUser(Guid id)
    {
        var user = BuildActiveUser(id);
        user.Deactivate();
        return user;
    }
}
