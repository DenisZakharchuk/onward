using Inventorization.Auth.BL.Entities;
using Inventorization.Auth.DTO.DTO.Role;
using Inventorization.Base.Abstractions;

namespace Inventorization.Auth.BL.Modifiers;

/// <summary>
/// Modifies Role entities based on UpdateRoleDTO
/// </summary>
public class RoleModifier : IEntityModifier<Role, UpdateRoleDTO>
{
    /// <summary>
    /// Modifies a Role entity from DTO
    /// </summary>
    public void Modify(Role entity, UpdateRoleDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        entity.Update(
            name: dto.Name,
            description: dto.Description
        );
    }
}
