using Onward.Auth.BL.Entities;
using Onward.Auth.DTO.DTO.Permission;
using Onward.Base.Abstractions;

namespace Onward.Auth.BL.Creators;

/// <summary>
/// Creates Permission entities from CreatePermissionDTO
/// </summary>
public class PermissionCreator : IEntityCreator<Permission, CreatePermissionDTO>
{
    /// <summary>
    /// Creates a new Permission entity from DTO
    /// </summary>
    public Permission Create(CreatePermissionDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Permission(
            name: dto.Name,
            resource: dto.Resource,
            action: dto.Action,
            description: dto.Description
        );
    }
}
