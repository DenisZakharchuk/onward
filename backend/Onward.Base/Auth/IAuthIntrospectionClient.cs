namespace Onward.Base.Auth;

/// <summary>
/// Client contract for calling the Auth Service's token introspection endpoint.
/// Implementations include HTTP, gRPC, and cached decorator variants.
/// </summary>
public interface IAuthIntrospectionClient
{
    /// <summary>
    /// Introspects a JWT by its unique identifier (jti claim).
    /// </summary>
    /// <param name="jti">The JWT ID claim value to introspect.</param>
    /// <param name="userId">The user ID extracted from the validated JWT principal.</param>
    /// <param name="tenantId">Optional tenant context. Pass <c>null</c> for single-tenant deployments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An <see cref="IntrospectionResult"/> indicating whether the token is active.
    /// </returns>
    Task<IntrospectionResult> IntrospectAsync(
        string jti,
        Guid userId,
        string? tenantId = null,
        CancellationToken cancellationToken = default);
}
