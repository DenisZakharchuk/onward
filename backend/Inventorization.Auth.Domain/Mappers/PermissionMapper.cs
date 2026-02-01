using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.DTO.DTO.Permission;
using Inventorization.Base.Abstractions;
using System.Linq.Expressions;

namespace Inventorization.Auth.Domain.Mappers;

/// <summary>
/// Mapper for Permission entity to PermissionDetailsDTO with both object and LINQ projection support
/// </summary>
public class PermissionMapper : IMapper<Permission, PermissionDetailsDTO>
{
    /// <summary>
    /// Maps a Permission entity to PermissionDetailsDTO
    /// </summary>
    public PermissionDetailsDTO Map(Permission entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new PermissionDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Resource = entity.Resource,
            Action = entity.Action,
            CreatedAt = entity.CreatedAt
        };
    }

    /// <summary>
    /// Provides LINQ expression for projection from Permission to PermissionDetailsDTO
    /// </summary>
    public Expression<Func<Permission, PermissionDetailsDTO>> GetProjection()
    {
        return permission => new PermissionDetailsDTO
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Resource = permission.Resource,
            Action = permission.Action,
            CreatedAt = permission.CreatedAt
        };
    }
}
