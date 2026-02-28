namespace Onward.Auth.DTO.DTO.Auth;

/// <summary>Request DTO for blocking a user account.</summary>
public sealed class BlockUserDTO : Onward.Base.DTOs.UpdateDTO
{
    /// <summary>Human-readable reason for blocking the user.</summary>
    public string Reason { get; set; } = "Account blocked by administrator.";
}
