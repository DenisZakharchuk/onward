using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Onward.Base.Auth;

namespace Onward.Base.AspNetCore.Auth;

/// <summary>
/// Caching decorator around <see cref="IAuthIntrospectionClient"/>.
/// Caches the <see cref="IntrospectionResult"/> per JTI for
/// <see cref="OnwardOnlineAuthSettings.CacheTtlSeconds"/> seconds using <see cref="IMemoryCache"/>.
/// </summary>
public sealed class CachedAuthIntrospectionClient : IAuthIntrospectionClient
{
    private readonly IAuthIntrospectionClient _inner;
    private readonly IMemoryCache _cache;
    private readonly OnwardOnlineAuthSettings _settings;
    private readonly ILogger<CachedAuthIntrospectionClient> _logger;

    // Cache key prefix to avoid collisions with other IMemoryCache consumers
    private const string KeyPrefix = "auth:jti:";

    public CachedAuthIntrospectionClient(
        IAuthIntrospectionClient inner,
        IMemoryCache cache,
        IOptions<OnwardOnlineAuthSettings> options,
        ILogger<CachedAuthIntrospectionClient> logger)
    {
        _inner = inner;
        _cache = cache;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<IntrospectionResult> IntrospectAsync(
        string jti,
        Guid userId,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        // Bypass cache when TTL is 0 (per-request mode)
        if (_settings.CacheTtlSeconds <= 0)
            return await _inner.IntrospectAsync(jti, userId, tenantId, cancellationToken);

        var key = KeyPrefix + jti;

        if (_cache.TryGetValue(key, out IntrospectionResult? cached) && cached is not null)
        {
            _logger.LogDebug("Cache hit for JTI {Jti}.", jti);
            return cached;
        }

        _logger.LogDebug("Cache miss for JTI {Jti}. Calling Auth Service.", jti);
        var result = await _inner.IntrospectAsync(jti, userId, tenantId, cancellationToken);

        // Only cache active results; inactive results (revoked, blocked) must always
        // be re-evaluated so that an unblock takes effect within the TTL window.
        if (result.Active)
        {
            var expiry = TimeSpan.FromSeconds(_settings.CacheTtlSeconds);
            _cache.Set(key, result, expiry);
        }

        return result;
    }
}
