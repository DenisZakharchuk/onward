namespace Onward.Auth.DTO.DTO.User;

public class UpdateUserDTO : Onward.Base.DTOs.UpdateDTO
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? NewPassword { get; set; }
    public bool? IsActive { get; set; }
    public ICollection<Guid>? RoleIds { get; set; }
}
