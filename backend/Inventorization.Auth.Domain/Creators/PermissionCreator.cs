using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.DTO.DTO.Permission;
using Inventorization.Base.Abstractions;

namespace Inventorization.Auth.Domain.Creators;

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
