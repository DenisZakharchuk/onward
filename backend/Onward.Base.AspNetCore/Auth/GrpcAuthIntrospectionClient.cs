using Onward.Base.Auth;

namespace Onward.Base.AspNetCore.Auth;

/// <summary>
/// gRPC-based implementation of <see cref="IAuthIntrospectionClient"/>.
/// <para>
/// This is a placeholder that signals correct wiring in DI.
/// Replace the body with a real gRPC channel + generated proto client
/// (e.g. via Grpc.Net.Client) when gRPC transport is required.
/// </para>
/// </summary>
public sealed class GrpcAuthIntrospectionClient : IAuthIntrospectionClient
{
    public Task<IntrospectionResult> IntrospectAsync(
        string jti,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: replace with real gRPC channel call once proto is defined.
        throw new NotImplementedException(
            "GrpcAuthIntrospectionClient is not yet implemented. " +
            "Define the auth.proto contract and regenerate the client, " +
            "then replace this body with the actual gRPC call.");
    }
}
