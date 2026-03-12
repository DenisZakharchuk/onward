namespace Onward.Base.Abstractions;

/// <summary>
/// Reads and writes idempotency-related HTTP tokens (ETag, If-Match, If-None-Match,
/// X-Idempotency-Key) without coupling the domain layer to ASP.NET Core.
/// </summary>
/// <remarks>
/// Implementations live in infrastructure layers (e.g. Onward.Base.AspNetCore).
/// Use <see cref="NoOpIdempotencyTokenAccessor"/> in non-HTTP contexts (background jobs, tests).
///
/// Token flow:
/// <list type="bullet">
///   <item>GET    → write ETag via <see cref="SetResponseToken"/></item>
///   <item>GET    → read If-None-Match via <see cref="GetConditionalToken"/> for 304 shortcut</item>
///   <item>PUT/DELETE → read If-Match via <see cref="GetMutationToken"/> for conflict detection</item>
///   <item>POST   → read X-Idempotency-Key via <see cref="GetIdempotencyKey"/> for deduplication</item>
/// </list>
/// </remarks>
public interface IIdempotencyTokenAccessor
{
    /// <summary>
    /// Returns the value of the <c>If-Match</c> header (quotes stripped), used to
    /// detect stale mutations on PUT and DELETE operations.
    /// Returns <c>null</c> when the header is absent — callers treat absence as "skip check".
    /// </summary>
    string? GetMutationToken();

    /// <summary>
    /// Returns the value of the <c>If-None-Match</c> header (quotes stripped), used to
    /// short-circuit GET responses with 304 Not Modified when the client's cached version
    /// matches the current entity token.
    /// Returns <c>null</c> when the header is absent.
    /// </summary>
    string? GetConditionalToken();

    /// <summary>
    /// Returns the value of the <c>X-Idempotency-Key</c> header, used to deduplicate
    /// POST (create) operations. Returns <c>null</c> when the header is absent.
    /// </summary>
    string? GetIdempotencyKey();

    /// <summary>
    /// Writes the given token as an <c>ETag</c> response header, wrapped in double quotes.
    /// No-op if the current execution context has no response (e.g. background jobs).
    /// </summary>
    void SetResponseToken(string token);
}

/// <summary>
/// No-op implementation of <see cref="IIdempotencyTokenAccessor"/> for use in non-HTTP
/// contexts (background jobs, unit tests, or when idempotency is disabled).
/// All reads return <c>null</c>; <see cref="SetResponseToken"/> is a no-op.
/// </summary>
public sealed class NoOpIdempotencyTokenAccessor : IIdempotencyTokenAccessor
{
    /// <summary>Singleton instance — safe to share across threads.</summary>
    public static readonly NoOpIdempotencyTokenAccessor Instance = new();

    private NoOpIdempotencyTokenAccessor() { }

    /// <inheritdoc/>
    public string? GetMutationToken() => null;

    /// <inheritdoc/>
    public string? GetConditionalToken() => null;

    /// <inheritdoc/>
    public string? GetIdempotencyKey() => null;

    /// <inheritdoc/>
    public void SetResponseToken(string token) { }
}
