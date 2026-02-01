namespace Inventorization.Auth.DTO.DTO.Role;

public class CreateRoleDTO : Inventorization.Base.DTOs.CreateDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<Guid> PermissionIds { get; set; } = new List<Guid>();
}
