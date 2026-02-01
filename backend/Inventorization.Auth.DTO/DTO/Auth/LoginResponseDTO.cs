namespace Inventorization.Auth.DTO.DTO.Auth;

public class LoginResponseDTO
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int ExpiresIn { get; set; } // Seconds
}
