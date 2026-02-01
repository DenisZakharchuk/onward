namespace Inventorization.Auth.DTO.DTO.Auth;

public class RefreshTokenRequestDTO : Inventorization.Base.DTOs.CreateDTO
{
    public string RefreshToken { get; set; } = null!;
}
