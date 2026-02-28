using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Onward.Base.Auth;

namespace Onward.Base.AspNetCore.Auth;

/// <summary>
/// Calls the Auth Service introspection endpoint over HTTP.
/// Registered as the inner (non-cached) implementation of <see cref="IAuthIntrospectionClient"/>.
/// </summary>
public sealed class HttpAuthIntrospectionClient : IAuthIntrospectionClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HttpAuthIntrospectionClient> _logger;

    public HttpAuthIntrospectionClient(
        HttpClient http,
        IHttpContextAccessor httpContextAccessor,
        ILogger<HttpAuthIntrospectionClient> logger)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<IntrospectionResult> IntrospectAsync(
        string jti,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        // Extract userId from the current request's JWT claims (already validated by ASP.NET Core)
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("Cannot introspect JTI {Jti}: userId not found in current request claims.", jti);
            return IntrospectionResult.InactiveResult("UserId not available for introspection.");
        }

        var payload = new IntrospectPayload(jti, userId, tenantId);

        var response = await _http.PostAsJsonAsync("api/tokens/introspect", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Auth Service returned {StatusCode} for JTI {Jti}.", response.StatusCode, jti);
            return IntrospectionResult.InactiveResult($"Auth Service returned {response.StatusCode}.");
        }

        var dto = await response.Content.ReadFromJsonAsync<ServiceResultEnvelope<IntrospectionResultPayload>>(
            cancellationToken: cancellationToken);

        if (dto?.Data is null || !dto.IsSuccess)
            return IntrospectionResult.InactiveResult(dto?.Message ?? "Auth Service returned no data.");

        return Map(dto.Data);
    }

    private static IntrospectionResult Map(IntrospectionResultPayload dto) =>
        dto.Active
            ? IntrospectionResult.ActiveResult(
                dto.UserId,
                dto.Email,
                dto.Roles.AsReadOnly(),
                dto.Permissions.AsReadOnly(),
                dto.TenantId)
            : IntrospectionResult.InactiveResult(dto.InactiveReason ?? "Token inactive.", dto.Blocked);

    // ── Private JSON payload types ─────────────────────────────────────────
    // Defined locally to avoid taking a dependency on Onward.Auth.DTO.

    private sealed record IntrospectPayload(string Jti, Guid UserId, string? TenantId);

    private sealed class IntrospectionResultPayload
    {
        public bool Active { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public bool Blocked { get; set; }
        public string? TenantId { get; set; }
        public string? InactiveReason { get; set; }
    }

    private sealed record ServiceResultEnvelope<T>(bool IsSuccess, T? Data, string? Message);
}
