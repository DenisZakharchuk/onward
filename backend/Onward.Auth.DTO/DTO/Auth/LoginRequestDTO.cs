namespace Onward.Auth.DTO.DTO.Auth;

public class LoginRequestDTO : Onward.Base.DTOs.CreateDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    /// <summary>
    /// Optional tenant identifier. When provided it is embedded as the
    /// <c>tenant_id</c> claim inside the issued access token.
    /// </summary>
    public string? TenantId { get; set; }
}
