using Inventorization.Auth.BL.DataAccess.Repositories;
using Inventorization.Auth.DTO.DTO.User;
using Inventorization.Base.Abstractions;
using System.Text.RegularExpressions;

namespace Inventorization.Auth.BL.Validators;

/// <summary>
/// Validates CreateUserDTO for user creation
/// </summary>
public class CreateUserDtoValidator : IValidator<CreateUserDTO>
{
    private readonly IUserRepository _userRepository;

    public CreateUserDtoValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Validates user creation data
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(CreateUserDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return ValidationResult.WithErrors("User data is required");

        var errors = new List<string>();

        // Validate email
        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add("Email is required");
        else if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errors.Add("Email format is invalid");
        else
        {
            // Check email uniqueness
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingUser != null)
                errors.Add("Email already exists");
        }

        // Validate full name
        if (string.IsNullOrWhiteSpace(dto.FullName))
            errors.Add("Full name is required");
        else if (dto.FullName.Length < 2)
            errors.Add("Full name must be at least 2 characters");

        // Validate password
        if (string.IsNullOrWhiteSpace(dto.Password))
            errors.Add("Password is required");
        else if (dto.Password.Length < 8)
            errors.Add("Password must be at least 8 characters");

        return errors.Any()
            ? ValidationResult.WithErrors(errors.ToArray())
            : ValidationResult.Ok();
    }
}
