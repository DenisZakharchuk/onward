using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.DTO.DTO.User;
using Inventorization.Base.Abstractions;
using System.Linq.Expressions;

namespace Inventorization.Auth.Domain.Mappers;

/// <summary>
/// Mapper for User entity to UserDetailsDTO with both object and LINQ projection support
/// </summary>
public class UserMapper : IMapper<User, UserDetailsDTO>
{
    /// <summary>
    /// Maps a User entity to UserDetailsDTO
    /// </summary>
    public UserDetailsDTO Map(User entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new UserDetailsDTO
        {
            Id = entity.Id,
            Email = entity.Email,
            FullName = entity.FullName,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            Roles = entity.UserRoles
                .Select(ur => new Inventorization.Auth.DTO.DTO.User.RoleInfoDTO
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name
                })
                .ToList(),
            Permissions = entity.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList()
        };
    }

    /// <summary>
    /// Provides LINQ expression for projection from User to UserDetailsDTO
    /// </summary>
    public Expression<Func<User, UserDetailsDTO>> GetProjection()
    {
        return user => new UserDetailsDTO
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles
                .Select(ur => new Inventorization.Auth.DTO.DTO.User.RoleInfoDTO
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name
                })
                .ToList(),
            Permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList()
        };
    }
}
