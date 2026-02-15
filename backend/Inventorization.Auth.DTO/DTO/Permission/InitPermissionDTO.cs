namespace Inventorization.Auth.DTO.DTO.Permission;

public record InitPermissionDTO(Guid Id, string Name, string Resource, string Action) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitPermissionDTO() : this(Guid.Empty, default!, default!, default!)
    {
    }
}
