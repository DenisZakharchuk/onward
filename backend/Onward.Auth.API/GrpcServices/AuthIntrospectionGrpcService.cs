using Grpc.Core;
using Onward.Auth.API.GrpcProto;
using Onward.Auth.BL.Services.Abstractions;

namespace Onward.Auth.API.GrpcServices;

/// <summary>
/// gRPC service implementation for token introspection.
/// Exposes <see cref="ITokenIntrospectionService"/> over gRPC for bounded contexts
/// configured with <c>OnlineAuth.Transport = "Grpc"</c>.
/// </summary>
public sealed class AuthIntrospectionGrpcService : AuthIntrospection.AuthIntrospectionBase
{
    private readonly ITokenIntrospectionService _introspectionService;
    private readonly ILogger<AuthIntrospectionGrpcService> _logger;

    public AuthIntrospectionGrpcService(
        ITokenIntrospectionService introspectionService,
        ILogger<AuthIntrospectionGrpcService> logger)
    {
        _introspectionService = introspectionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public override async Task<IntrospectResponse> IntrospectToken(
        IntrospectRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Jti))
        {
            _logger.LogWarning("gRPC IntrospectToken called with empty JTI.");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "jti is required."));
        }

        if (!Guid.TryParse(request.UserId, out var userId))
        {
            _logger.LogWarning("gRPC IntrospectToken called with invalid user_id: {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "user_id must be a valid GUID."));
        }

        var result = await _introspectionService.IntrospectAsync(
            request.Jti,
            userId,
            request.HasTenantId ? request.TenantId : null,
            context.CancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            _logger.LogWarning("Introspection service returned failure for JTI {Jti}: {Message}", request.Jti, result.Message);
            return new IntrospectResponse { Active = false, InactiveReason = result.Message ?? "Introspection failed." };
        }

        var dto = result.Data;
        var response = new IntrospectResponse
        {
            Active  = dto.Active,
            UserId  = dto.UserId.ToString(),
            Email   = dto.Email,
            Blocked = dto.Blocked,
        };

        response.Roles.AddRange(dto.Roles);
        response.Permissions.AddRange(dto.Permissions);

        if (dto.TenantId is not null)
            response.TenantId = dto.TenantId;

        if (!dto.Active && dto.InactiveReason is not null)
            response.InactiveReason = dto.InactiveReason;

        return response;
    }
}
