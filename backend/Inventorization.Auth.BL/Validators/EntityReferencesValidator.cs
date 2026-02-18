using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.DTOs;
using Inventorization.Auth.BL.Entities;

namespace Inventorization.Auth.BL.Validators;

/// <summary>
/// Validates entity reference changes for relationship updates
/// </summary>
public class EntityReferencesValidator : IValidator<EntityReferencesDTO>
{
    private readonly IRepository<Role> _roleRepository;

    public EntityReferencesValidator(IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    }

    public async Task<ValidationResult> ValidateAsync(
        EntityReferencesDTO dto,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (dto == null)
        {
            errors.Add("EntityReferencesDTO is required");
            return ValidationResult.WithErrors(errors.ToArray());
        }

        // Validate entities to add exist
        if (dto.IdsToAdd != null && dto.IdsToAdd.Any())
        {
            foreach (var roleId in dto.IdsToAdd)
            {
                if (roleId == Guid.Empty)
                {
                    errors.Add("Invalid role ID (empty GUID)");
                    continue;
                }

                var exists = await _roleRepository.ExistsAsync(roleId, cancellationToken);
                if (!exists)
                    errors.Add($"Role {roleId} not found");
            }
        }

        // Business rules
        if (dto.IdsToAdd != null && dto.IdsToAdd.Count > 10)
            errors.Add("Cannot assign more than 10 roles at once");

        // Validate entities to remove
        if (dto.IdsToRemove != null && dto.IdsToRemove.Any())
        {
            foreach (var roleId in dto.IdsToRemove)
            {
                if (roleId == Guid.Empty)
                {
                    errors.Add("Invalid role ID to remove (empty GUID)");
                }
            }
        }

        return errors.Any()
            ? ValidationResult.WithErrors(errors.ToArray())
            : ValidationResult.Ok();
    }
}
