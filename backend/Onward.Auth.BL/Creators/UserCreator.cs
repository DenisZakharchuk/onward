using Onward.Auth.BL.Entities;
using Onward.Auth.DTO.DTO.User;
using Onward.Base.Abstractions;

namespace Onward.Auth.BL.Creators;

/// <summary>
/// Creates User entities from CreateUserDTO
/// </summary>
public class UserCreator : IEntityCreator<User, CreateUserDTO>
{
    private readonly IPasswordHasher _passwordHasher;

    public UserCreator(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <summary>
    /// Creates a new User entity from DTO with password hashing
    /// </summary>
    public User Create(CreateUserDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var hashedPassword = _passwordHasher.HashPassword(dto.Password);

        return new User(
            email: dto.Email,
            passwordHash: hashedPassword,
            fullName: dto.FullName
        );
    }
}
