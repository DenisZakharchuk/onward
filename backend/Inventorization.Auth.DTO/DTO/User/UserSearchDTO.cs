namespace Inventorization.Auth.DTO.DTO.User;

public class UserSearchDTO : Inventorization.Base.DTOs.SearchDTO
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool? IsActive { get; set; }
}
