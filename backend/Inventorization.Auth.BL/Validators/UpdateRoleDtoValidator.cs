using Inventorization.Auth.DTO.DTO.Role;
using Inventorization.Base.Abstractions;

namespace Inventorization.Auth.BL.Validators;

/// <summary>
/// Validates UpdateRoleDTO for role updates
/// </summary>
public class UpdateRoleDtoValidator : IValidator<UpdateRoleDTO>
{
    /// <summary>
    /// Validates role update data
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(UpdateRoleDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return ValidationResult.WithErrors("Role data is required");

        var errors = new List<string>();

        // Validate name
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Role name is required");
        else if (dto.Name.Length < 2)
            errors.Add("Role name must be at least 2 characters");

        return errors.Any()
            ? ValidationResult.WithErrors(errors.ToArray())
            : ValidationResult.Ok();
    }
}
