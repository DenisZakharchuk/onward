using Inventorization.Base.Abstractions;
using InventorySystem.DTOs.DTO.StockMovement;

namespace InventorySystem.Business.Validators;

/// <summary>
/// Validator for CreateStockMovementDTO
/// </summary>
public class CreateStockMovementValidator : IValidator<CreateStockMovementDTO>
{
    public Task<ValidationResult> ValidateAsync(CreateStockMovementDTO obj, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (obj.ProductId == Guid.Empty)
            errors.Add("Product ID is required.");

        if (obj.Quantity <= 0)
            errors.Add("Quantity must be greater than zero.");

        return Task.FromResult(
            errors.Count == 0 ? ValidationResult.Ok() : ValidationResult.WithErrors(errors.ToArray())
        );
    }
}
