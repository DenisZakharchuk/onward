using Inventorization.Auth.BL.Entities;
using Inventorization.Auth.DTO.DTO.Role;
using Inventorization.Base.Abstractions;

namespace Inventorization.Auth.BL.Creators;

/// <summary>
/// Creates Role entities from CreateRoleDTO
/// </summary>
public class RoleCreator : IEntityCreator<Role, CreateRoleDTO>
{
    /// <summary>
    /// Creates a new Role entity from DTO
    /// </summary>
    public Role Create(CreateRoleDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Role(
            name: dto.Name,
            description: dto.Description
        );
    }
}
