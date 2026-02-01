using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.DTO.DTO.User;
using Inventorization.Base.Abstractions;

namespace Inventorization.Auth.Domain.Modifiers;

/// <summary>
/// Modifies User entities based on UpdateUserDTO
/// </summary>
public class UserModifier : IEntityModifier<User, UpdateUserDTO>
{
    private readonly IPasswordHasher _passwordHasher;

    public UserModifier(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <summary>
    /// Modifies a User entity from DTO
    /// </summary>
    public void Modify(User entity, UpdateUserDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        // Update profile information
        entity.UpdateProfile(dto.Email, dto.FullName);

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            var hashedPassword = _passwordHasher.HashPassword(dto.NewPassword);
            entity.SetPassword(hashedPassword);
        }

        // Update active status if specified
        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value)
                entity.Activate();
            else
                entity.Deactivate();
        }
    }
}
