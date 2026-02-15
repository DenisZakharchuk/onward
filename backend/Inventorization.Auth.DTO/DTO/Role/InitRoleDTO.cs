namespace Inventorization.Auth.DTO.DTO.Role;

public record InitRoleDTO(Guid Id, string Name) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitRoleDTO() : this(Guid.Empty, default!)
    {
    }
}
