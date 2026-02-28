namespace Onward.Auth.DTO.DTO.Auth;

public class RefreshTokenRequestDTO : Onward.Base.DTOs.CreateDTO
{
    public string RefreshToken { get; set; } = null!;
}
