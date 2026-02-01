namespace Inventorization.Auth.DTO.DTO.Role;

public class UpdateRoleDTO : Inventorization.Base.DTOs.UpdateDTO
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Guid>? PermissionIds { get; set; }
}
