using Inventorization.Auth.BL.Entities;
using Inventorization.Auth.DTO.DTO.Role;
using Inventorization.Base.Abstractions;
using System.Linq.Expressions;

namespace Inventorization.Auth.BL.Mappers;

/// <summary>
/// Mapper for Role entity to RoleDetailsDTO with both object and LINQ projection support
/// </summary>
public class RoleMapper : IMapper<Role, RoleDetailsDTO>
{
    /// <summary>
    /// Maps a Role entity to RoleDetailsDTO
    /// </summary>
    public RoleDetailsDTO Map(Role entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new RoleDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            Permissions = entity.RolePermissions
                .Select(rp => new Inventorization.Auth.DTO.DTO.Role.PermissionInfoDTO
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Resource = rp.Permission.Resource,
                    Action = rp.Permission.Action
                })
                .ToList()
        };
    }

    /// <summary>
    /// Provides LINQ expression for projection from Role to RoleDetailsDTO
    /// </summary>
    public Expression<Func<Role, RoleDetailsDTO>> GetProjection()
    {
        return role => new RoleDetailsDTO
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            Permissions = role.RolePermissions
                .Select(rp => new Inventorization.Auth.DTO.DTO.Role.PermissionInfoDTO
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Resource = rp.Permission.Resource,
                    Action = rp.Permission.Action
                })
                .ToList()
        };
    }
}
