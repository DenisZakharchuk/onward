namespace Onward.Auth.DTO.DTO.Auth;

public class LoginRequestDTO : Onward.Base.DTOs.CreateDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
