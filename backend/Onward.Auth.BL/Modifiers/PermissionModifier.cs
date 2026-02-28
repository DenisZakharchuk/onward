using Onward.Auth.BL.Entities;
using Onward.Auth.DTO.DTO.Permission;
using Onward.Base.Abstractions;

namespace Onward.Auth.BL.Modifiers;

/// <summary>
/// Modifies Permission entities based on UpdatePermissionDTO
/// </summary>
public class PermissionModifier : IEntityModifier<Permission, UpdatePermissionDTO>
{
    /// <summary>
    /// Modifies a Permission entity from DTO
    /// </summary>
    public void Modify(Permission entity, UpdatePermissionDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        entity.Update(
            name: dto.Name,
            resource: dto.Resource,
            action: dto.Action,
            description: dto.Description
        );
    }
}
