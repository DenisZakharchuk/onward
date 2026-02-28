using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Onward.Base.Auth;
using Onward.Base.AspNetCore.Auth;

namespace Onward.Auth.API.Tests.Services;

public class CachedAuthIntrospectionClientTests
{
    private static IOptions<OnwardOnlineAuthSettings> Options(int ttl) =>
        Microsoft.Extensions.Options.Options.Create(new OnwardOnlineAuthSettings { CacheTtlSeconds = ttl });

    private static IMemoryCache NewCache() =>
        new MemoryCache(new MemoryCacheOptions());

    private static ILogger<CachedAuthIntrospectionClient> Logger() =>
        Mock.Of<ILogger<CachedAuthIntrospectionClient>>();

    // ── Cache hit skips inner call ─────────────────────────────────────────

    [Fact]
    public async Task IntrospectAsync_CacheHit_DoesNotCallInner()
    {
        var inner = new Mock<IAuthIntrospectionClient>();
        inner.Setup(i => i.IntrospectAsync("jti", null, default))
             .ReturnsAsync(IntrospectionResult.ActiveResult(Guid.NewGuid(), "a@b.com",
                 new[] { "Admin" }.AsReadOnly(), Array.Empty<string>().AsReadOnly()));

        var sut = new CachedAuthIntrospectionClient(inner.Object, NewCache(), Options(30), Logger());

        // First call — populates cache
        var first = await sut.IntrospectAsync("jti");
        // Second call — should hit cache
        var second = await sut.IntrospectAsync("jti");

        Assert.True(first.Active);
        Assert.True(second.Active);
        inner.Verify(i => i.IntrospectAsync("jti", null, default), Times.Once);
    }

    // ── Different JTIs get separate cache entries ──────────────────────────

    [Fact]
    public async Task IntrospectAsync_DifferentJtis_AreIndependent()
    {
        var inner = new Mock<IAuthIntrospectionClient>();
        inner.Setup(i => i.IntrospectAsync(It.IsAny<string>(), null, default))
             .ReturnsAsync(IntrospectionResult.ActiveResult(Guid.NewGuid(), "a@b.com",
                 Array.Empty<string>().AsReadOnly(), Array.Empty<string>().AsReadOnly()));

        var cache = NewCache();
        var sut = new CachedAuthIntrospectionClient(inner.Object, cache, Options(30), Logger());

        await sut.IntrospectAsync("jti-A");
        await sut.IntrospectAsync("jti-B");

        inner.Verify(i => i.IntrospectAsync("jti-A", null, default), Times.Once);
        inner.Verify(i => i.IntrospectAsync("jti-B", null, default), Times.Once);
    }

    // ── Inactive results are NOT cached ───────────────────────────────────

    [Fact]
    public async Task IntrospectAsync_InactiveResult_NotCached()
    {
        var inner = new Mock<IAuthIntrospectionClient>();
        inner.Setup(i => i.IntrospectAsync("bad-jti", null, default))
             .ReturnsAsync(IntrospectionResult.InactiveResult("revoked"));

        var sut = new CachedAuthIntrospectionClient(inner.Object, NewCache(), Options(30), Logger());

        await sut.IntrospectAsync("bad-jti");
        await sut.IntrospectAsync("bad-jti");

        // Inner called twice — inactive results bypass cache so an unblock is seen immediately
        inner.Verify(i => i.IntrospectAsync("bad-jti", null, default), Times.Exactly(2));
    }

    // ── TTL=0 disables cache ───────────────────────────────────────────────

    [Fact]
    public async Task IntrospectAsync_ZeroTtl_AlwaysCallsInner()
    {
        var inner = new Mock<IAuthIntrospectionClient>();
        inner.Setup(i => i.IntrospectAsync("jti", null, default))
             .ReturnsAsync(IntrospectionResult.ActiveResult(Guid.NewGuid(), "a@b.com",
                 Array.Empty<string>().AsReadOnly(), Array.Empty<string>().AsReadOnly()));

        var sut = new CachedAuthIntrospectionClient(inner.Object, NewCache(), Options(0), Logger());

        await sut.IntrospectAsync("jti");
        await sut.IntrospectAsync("jti");

        inner.Verify(i => i.IntrospectAsync("jti", null, default), Times.Exactly(2));
    }
}
