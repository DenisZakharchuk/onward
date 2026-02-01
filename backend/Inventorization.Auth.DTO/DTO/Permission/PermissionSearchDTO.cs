namespace Inventorization.Auth.DTO.DTO.Permission;

public class PermissionSearchDTO : Inventorization.Base.DTOs.SearchDTO
{
    public string? Name { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
}
