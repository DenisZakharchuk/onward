namespace Onward.Auth.DTO.DTO.Auth;

/// <summary>Request DTO for explicitly revoking a single access token by its JTI.</summary>
public sealed class RevokeTokenDTO : Onward.Base.DTOs.CreateDTO
{
    /// <summary>The JWT unique identifier (jti claim) to revoke.</summary>
    public string Jti { get; set; } = null!;

    /// <summary>Optional human-readable reason for the revocation.</summary>
    public string? Reason { get; set; }
}
