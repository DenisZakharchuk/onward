namespace Onward.Auth.DTO.DTO.Auth;

/// <summary>Request DTO for the token introspection endpoint (service-to-service call).</summary>
public sealed class IntrospectRequestDTO : Onward.Base.DTOs.CreateDTO
{
    /// <summary>The JWT unique identifier (jti claim) to introspect.</summary>
    public string Jti { get; set; } = null!;

    /// <summary>The user ID extracted from the JWT (NameIdentifier claim).</summary>
    public Guid UserId { get; set; }

    /// <summary>Optional tenant context from the token.</summary>
    public string? TenantId { get; set; }
}
