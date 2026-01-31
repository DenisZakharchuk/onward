using Inventorization.Base.Abstractions;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.Business.Validators;

/// <summary>
/// Validator for CreateCategoryDTO
/// </summary>
public class CreateCategoryValidator : IValidator<CreateCategoryDTO>
{
    public Task<ValidationResult> ValidateAsync(CreateCategoryDTO obj, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(obj.Name))
            errors.Add("Category name is required.");

        if (obj.Name?.Length > 100)
            errors.Add("Category name cannot exceed 100 characters.");

        return Task.FromResult(
            errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.WithErrors(errors.ToArray())
        );
    }
}

/// <summary>
/// Validator for UpdateCategoryDTO
/// </summary>
public class UpdateCategoryValidator : IValidator<UpdateCategoryDTO>
{
    public Task<ValidationResult> ValidateAsync(UpdateCategoryDTO obj, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (obj.Id == Guid.Empty)
            errors.Add("Category ID is required.");

        if (string.IsNullOrWhiteSpace(obj.Name))
            errors.Add("Category name is required.");

        if (obj.Name?.Length > 100)
            errors.Add("Category name cannot exceed 100 characters.");

        return Task.FromResult(
            errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.WithErrors(errors.ToArray())
        );
    }
}
