namespace Inventorization.Auth.DTO.DTO.Auth;

public class LoginRequestDTO : Inventorization.Base.DTOs.CreateDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
