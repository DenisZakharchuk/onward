using Inventorization.Auth.Domain.DataAccess.Repositories;
using Inventorization.Auth.DTO.DTO.User;
using Inventorization.Base.Abstractions;
using System.Text.RegularExpressions;

namespace Inventorization.Auth.Domain.Validators;

/// <summary>
/// Validates UpdateUserDTO for user updates
/// </summary>
public class UpdateUserDtoValidator : IValidator<UpdateUserDTO>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserDtoValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Validates user update data
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(UpdateUserDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return ValidationResult.WithErrors("User data is required");

        var errors = new List<string>();

        // Validate email
        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add("Email is required");
        else if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errors.Add("Email format is invalid");

        // Validate full name
        if (string.IsNullOrWhiteSpace(dto.FullName))
            errors.Add("Full name is required");
        else if (dto.FullName.Length < 2)
            errors.Add("Full name must be at least 2 characters");

        // Validate new password if provided
        if (!string.IsNullOrWhiteSpace(dto.NewPassword) && dto.NewPassword.Length < 8)
            errors.Add("New password must be at least 8 characters");

        return errors.Any()
            ? ValidationResult.WithErrors(errors.ToArray())
            : ValidationResult.Ok();
    }
}
