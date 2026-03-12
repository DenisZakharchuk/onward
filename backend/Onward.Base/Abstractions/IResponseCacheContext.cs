using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Onward.Base.Abstractions;

/// <summary>
/// Provides a backing store for response caching (GET results keyed by entity ID)
/// and POST idempotency deduplication (results keyed by X-Idempotency-Key).
/// </summary>
/// <remarks>
/// This interface intentionally operates on raw key strings so that callers are
/// fully decoupled from cache topology.  Three built-in implementations are provided:
/// <list type="bullet">
///   <item><see cref="NoOpResponseCacheContext"/> — always misses, never stores (default when mode=none)</item>
///   <item><see cref="InMemoryResponseCacheContext"/> — backed by <see cref="IMemoryCache"/> (mode=inmemory)</item>
///   <item><see cref="DistributedResponseCacheContext"/> — backed by <see cref="IDistributedCache"/> (mode=distributed)</item>
/// </list>
/// </remarks>
public interface IResponseCacheContext
{
    /// <summary>
    /// Attempts to retrieve a previously cached result for the given key.
    /// </summary>
    /// <typeparam name="T">The cached result type.</typeparam>
    /// <param name="key">Cache key (entity ID or idempotency key).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple of (hit, result, token) where:
    /// <list type="bullet">
    ///   <item><c>hit</c> — whether the key was found in cache</item>
    ///   <item><c>result</c> — the cached value, or <c>default</c> on a miss</item>
    ///   <item><c>token</c> — the concurrency token stored alongside the value, or <c>null</c></item>
    /// </list>
    /// </returns>
    Task<(bool hit, T? result, string? token)> TryGetAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a result in the cache under the given key, optionally alongside a concurrency token.
    /// </summary>
    /// <typeparam name="T">The result type to cache.</typeparam>
    /// <param name="key">Cache key (entity ID or idempotency key).</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="token">Optional concurrency token (e.g. ETag value) stored alongside the value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(
        string key,
        T value,
        string? token = null,
        CancellationToken cancellationToken = default);
}

// ---------------------------------------------------------------------------
// No-op implementation
// ---------------------------------------------------------------------------

/// <summary>
/// No-op implementation of <see cref="IResponseCacheContext"/>.
/// Always reports a cache miss; <see cref="SetAsync{T}"/> is a no-op.
/// Used when idempotency mode is 'none' or cache backing is disabled.
/// </summary>
public sealed class NoOpResponseCacheContext : IResponseCacheContext
{
    /// <summary>Singleton instance — safe to share across threads.</summary>
    public static readonly NoOpResponseCacheContext Instance = new();

    private NoOpResponseCacheContext() { }

    /// <inheritdoc/>
    public Task<(bool hit, T? result, string? token)> TryGetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
        => Task.FromResult<(bool, T?, string?)>((false, default, null));

    /// <inheritdoc/>
    public Task SetAsync<T>(
        string key,
        T value,
        string? token = null,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

// ---------------------------------------------------------------------------
// In-memory implementation
// ---------------------------------------------------------------------------

/// <summary>
/// In-process <see cref="IResponseCacheContext"/> backed by <see cref="IMemoryCache"/>.
/// Suitable for single-instance deployments (mode=inmemory). Not shared across pods.
/// </summary>
public sealed class InMemoryResponseCacheContext : IResponseCacheContext
{
    private readonly IMemoryCache _cache;

    public InMemoryResponseCacheContext(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc/>
    public Task<(bool hit, T? result, string? token)> TryGetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey(key), out CachedEntry<T>? entry) && entry is not null)
            return Task.FromResult<(bool, T?, string?)>((true, entry.Value, entry.Token));

        return Task.FromResult<(bool, T?, string?)>((false, default, null));
    }

    /// <inheritdoc/>
    public Task SetAsync<T>(
        string key,
        T value,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        _cache.Set(CacheKey(key), new CachedEntry<T>(value, token));
        return Task.CompletedTask;
    }

    private static string CacheKey(string key) => $"idem:{key}";

    private sealed record CachedEntry<T>(T? Value, string? Token);
}

// ---------------------------------------------------------------------------
// Distributed implementation
// ---------------------------------------------------------------------------

/// <summary>
/// Distributed <see cref="IResponseCacheContext"/> backed by <see cref="IDistributedCache"/>.
/// Suitable for multi-instance deployments using Redis or similar (mode=distributed).
/// Values are JSON-serialized.
/// </summary>
public sealed class DistributedResponseCacheContext : IResponseCacheContext
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public DistributedResponseCacheContext(IDistributedCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc/>
    public async Task<(bool hit, T? result, string? token)> TryGetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(CacheKey(key), cancellationToken);
        if (bytes is null)
            return (false, default, null);

        var entry = JsonSerializer.Deserialize<DistributedCachedEntry<T>>(bytes, _jsonOptions);
        return entry is not null
            ? (true, entry.Value, entry.Token)
            : (false, default, null);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(
        string key,
        T value,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        var entry = new DistributedCachedEntry<T>(value, token);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(entry, _jsonOptions);
        await _cache.SetAsync(CacheKey(key), bytes, cancellationToken);
    }

    private static string CacheKey(string key) => $"idem:{key}";

    private sealed record DistributedCachedEntry<T>(T? Value, string? Token);
}
