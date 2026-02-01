using Inventorization.Base.DTOs;

namespace Inventorization.Auth.DTO.DTO.Permission;

public class CreatePermissionDTO : CreateDTO
{
    public string Name { get; set; } = null!;
    public string Resource { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string? Description { get; set; }
}
