namespace Onward.Auth.DTO.DTO.Role;

public record InitRoleDTO(Guid Id, string Name) : Onward.Base.DTOs.InitDTO(Id)
{
    public InitRoleDTO() : this(Guid.Empty, default!)
    {
    }
}
