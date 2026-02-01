namespace Inventorization.Auth.DTO.DTO.User;

public class CreateUserDTO : Inventorization.Base.DTOs.CreateDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public ICollection<Guid> RoleIds { get; set; } = new List<Guid>();
}
