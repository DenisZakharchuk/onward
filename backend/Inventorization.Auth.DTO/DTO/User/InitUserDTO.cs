namespace Inventorization.Auth.DTO.DTO.User;

public record InitUserDTO(Guid Id, string Email, string Password, string FullName) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitUserDTO() : this(Guid.Empty, default!, default!, default!)
    {
    }
}
