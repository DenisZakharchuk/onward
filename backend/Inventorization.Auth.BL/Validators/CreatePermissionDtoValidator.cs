using Inventorization.Auth.DTO.DTO.Permission;
using Inventorization.Base.Abstractions;

namespace Inventorization.Auth.BL.Validators;

/// <summary>
/// Validates CreatePermissionDTO for permission creation
/// </summary>
public class CreatePermissionDtoValidator : IValidator<CreatePermissionDTO>
{
    /// <summary>
    /// Validates permission creation data
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(CreatePermissionDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return ValidationResult.WithErrors("Permission data is required");

        var errors = new List<string>();

        // Validate name
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Permission name is required");
        else if (dto.Name.Length < 2)
            errors.Add("Permission name must be at least 2 characters");

        // Validate resource
        if (string.IsNullOrWhiteSpace(dto.Resource))
            errors.Add("Resource is required");
        else if (dto.Resource.Length < 2)
            errors.Add("Resource must be at least 2 characters");

        // Validate action
        if (string.IsNullOrWhiteSpace(dto.Action))
            errors.Add("Action is required");
        else if (dto.Action.Length < 2)
            errors.Add("Action must be at least 2 characters");

        return errors.Any()
            ? ValidationResult.WithErrors(errors.ToArray())
            : ValidationResult.Ok();
    }
}
