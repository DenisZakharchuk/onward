using Inventorization.Base.Abstractions;
using InventorySystem.DTOs.DTO.Product;

namespace InventorySystem.Business.Validators;

/// <summary>
/// Validator for CreateProductDTO
/// </summary>
public class CreateProductValidator : IValidator<CreateProductDTO>
{
    public Task<ValidationResult> ValidateAsync(CreateProductDTO obj, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(obj.Name))
            errors.Add("Product name is required.");

        if (obj.Name?.Length > 200)
            errors.Add("Product name cannot exceed 200 characters.");

        if (obj.Price < 0)
            errors.Add("Product price cannot be negative.");

        if (obj.CategoryId == Guid.Empty)
            errors.Add("Category ID is required.");

        if (obj.MinimumStock < 0)
            errors.Add("Minimum stock cannot be negative.");

        if (obj.InitialStock < 0)
            errors.Add("Initial stock cannot be negative.");

        return Task.FromResult(
            errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.WithErrors(errors.ToArray())
        );
    }
}

/// <summary>
/// Validator for UpdateProductDTO
/// </summary>
public class UpdateProductValidator : IValidator<UpdateProductDTO>
{
    public Task<ValidationResult> ValidateAsync(UpdateProductDTO obj, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (obj.Id == Guid.Empty)
            errors.Add("Product ID is required.");

        if (string.IsNullOrWhiteSpace(obj.Name))
            errors.Add("Product name is required.");

        if (obj.Name?.Length > 200)
            errors.Add("Product name cannot exceed 200 characters.");

        if (obj.Price < 0)
            errors.Add("Product price cannot be negative.");

        if (obj.CategoryId == Guid.Empty)
            errors.Add("Category ID is required.");

        if (obj.MinimumStock < 0)
            errors.Add("Minimum stock cannot be negative.");

        return Task.FromResult(
            errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.WithErrors(errors.ToArray())
        );
    }
}
