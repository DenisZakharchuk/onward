namespace Inventorization.Auth.DTO.DTO.Role;

public class PermissionInfoDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Resource { get; set; } = null!;
    public string Action { get; set; } = null!;
}

public class RoleDetailsDTO : Inventorization.Base.DTOs.DetailsDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<PermissionInfoDTO> Permissions { get; set; } = new List<PermissionInfoDTO>();
}
