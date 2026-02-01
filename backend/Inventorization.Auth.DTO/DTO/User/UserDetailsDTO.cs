namespace Inventorization.Auth.DTO.DTO.User;

public class RoleInfoDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class UserDetailsDTO : Inventorization.Base.DTOs.DetailsDTO
{
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<RoleInfoDTO> Roles { get; set; } = new List<RoleInfoDTO>();
    public ICollection<string> Permissions { get; set; } = new List<string>();
}
