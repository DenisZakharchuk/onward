namespace Onward.Auth.DTO.DTO.Role;

public class CreateRoleDTO : Onward.Base.DTOs.CreateDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<Guid> PermissionIds { get; set; } = new List<Guid>();
}
