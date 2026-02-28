namespace Onward.Auth.DTO.DTO.Permission;

public class PermissionSearchDTO : Onward.Base.DTOs.SearchDTO
{
    public string? Name { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
}
