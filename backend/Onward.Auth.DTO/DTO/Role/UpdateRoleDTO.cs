namespace Onward.Auth.DTO.DTO.Role;

public class UpdateRoleDTO : Onward.Base.DTOs.UpdateDTO
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Guid>? PermissionIds { get; set; }
}
