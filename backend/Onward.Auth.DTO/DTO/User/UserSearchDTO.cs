namespace Onward.Auth.DTO.DTO.User;

public class UserSearchDTO : Onward.Base.DTOs.SearchDTO
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool? IsActive { get; set; }
}
