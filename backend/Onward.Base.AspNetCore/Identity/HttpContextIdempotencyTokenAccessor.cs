using Microsoft.AspNetCore.Http;
using Onward.Base.Abstractions;

namespace Onward.Base.AspNetCore.Identity;

/// <summary>
/// ASP.NET Core implementation of <see cref="IIdempotencyTokenAccessor"/>.
/// Reads standard idempotency-related HTTP request headers and writes the ETag response header.
/// Register as <c>AddScoped&lt;IIdempotencyTokenAccessor, HttpContextIdempotencyTokenAccessor&gt;()</c>.
/// </summary>
public sealed class HttpContextIdempotencyTokenAccessor : IIdempotencyTokenAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextIdempotencyTokenAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc/>
    /// <remarks>Reads the <c>If-Match</c> request header; strips surrounding double-quote characters.</remarks>
    public string? GetMutationToken()
    {
        var value = _httpContextAccessor.HttpContext?.Request.Headers.IfMatch.ToString();
        return string.IsNullOrEmpty(value) ? null : value.Trim('"');
    }

    /// <inheritdoc/>
    /// <remarks>Reads the <c>If-None-Match</c> request header; strips surrounding double-quote characters.</remarks>
    public string? GetConditionalToken()
    {
        var value = _httpContextAccessor.HttpContext?.Request.Headers.IfNoneMatch.ToString();
        return string.IsNullOrEmpty(value) ? null : value.Trim('"');
    }

    /// <inheritdoc/>
    /// <remarks>Reads the <c>X-Idempotency-Key</c> request header.</remarks>
    public string? GetIdempotencyKey()
    {
        var value = _httpContextAccessor.HttpContext?.Request.Headers["X-Idempotency-Key"].ToString();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    /// <inheritdoc/>
    /// <remarks>Sets the <c>ETag</c> response header wrapped in double quotes.</remarks>
    public void SetResponseToken(string token)
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response is null || response.HasStarted) return;

        response.Headers.ETag = $"\"{token}\"";
    }
}
