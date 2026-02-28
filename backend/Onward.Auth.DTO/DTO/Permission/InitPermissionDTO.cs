namespace Onward.Auth.DTO.DTO.Permission;

public record InitPermissionDTO(Guid Id, string Name, string Resource, string Action) : Onward.Base.DTOs.InitDTO(Id)
{
    public InitPermissionDTO() : this(Guid.Empty, default!, default!, default!)
    {
    }
}
