using Onward.Auth.BL.Entities;
using Onward.Auth.DTO.DTO.Role;
using Onward.Base.Abstractions;

namespace Onward.Auth.BL.Modifiers;

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
