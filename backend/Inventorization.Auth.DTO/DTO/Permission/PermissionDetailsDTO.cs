namespace Inventorization.Auth.DTO.DTO.Permission;

public class PermissionDetailsDTO : Inventorization.Base.DTOs.DetailsDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Resource { get; set; } = null!;
    public string Action { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
