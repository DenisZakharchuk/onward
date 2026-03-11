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
    private readonly ILogger<GrpcAuthIntrospectionClient> _logger;

    public GrpcAuthIntrospectionClient(
        AuthIntrospection.AuthIntrospectionClient grpcClient,
        ILogger<GrpcAuthIntrospectionClient> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IntrospectionResult> IntrospectAsync(
        string jti,
        Guid userId,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new IntrospectRequest
        {
            Jti    = jti,
            UserId = userId.ToString(),
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
