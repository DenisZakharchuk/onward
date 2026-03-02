using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Onward.Base.Auth;
using Onward.Base.AspNetCore.GrpcProto;

namespace Onward.Base.AspNetCore.Auth;

/// <summary>
/// gRPC-based implementation of <see cref="IAuthIntrospectionClient"/>.
/// Calls the Auth Service's <c>AuthIntrospection.IntrospectToken</c> RPC
/// via a pre-configured <see cref="AuthIntrospection.AuthIntrospectionClient"/> channel.
/// </summary>
public sealed class GrpcAuthIntrospectionClient : IAuthIntrospectionClient
{
    private readonly AuthIntrospection.AuthIntrospectionClient _grpcClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GrpcAuthIntrospectionClient> _logger;

    public GrpcAuthIntrospectionClient(
        AuthIntrospection.AuthIntrospectionClient grpcClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<GrpcAuthIntrospectionClient> logger)
    {
        _grpcClient = grpcClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IntrospectionResult> IntrospectAsync(
        string jti,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr))
        {
            _logger.LogWarning("Cannot introspect JTI {Jti} via gRPC: userId not found in current request claims.", jti);
            return IntrospectionResult.InactiveResult("UserId not available for introspection.");
        }

        var request = new IntrospectRequest
        {
            Jti    = jti,
            UserId = userIdStr,
        };

        if (tenantId is not null)
            request.TenantId = tenantId;

        try
        {
            var reply = await _grpcClient.IntrospectTokenAsync(request, cancellationToken: cancellationToken);

            if (!reply.Active)
                return IntrospectionResult.InactiveResult(
                    reply.HasInactiveReason ? reply.InactiveReason : "Token inactive.",
                    reply.Blocked);

            return IntrospectionResult.ActiveResult(
                Guid.Parse(reply.UserId),
                reply.Email,
                reply.Roles.ToList().AsReadOnly(),
                reply.Permissions.ToList().AsReadOnly(),
                reply.HasTenantId ? reply.TenantId : null);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger.LogError(ex, "gRPC introspection call failed for JTI {Jti}: {Status}", jti, ex.Status);
            throw; // Let OnwardOnlineJwtBearerEventsHandler handle FailOpen logic
        }
    }
}
