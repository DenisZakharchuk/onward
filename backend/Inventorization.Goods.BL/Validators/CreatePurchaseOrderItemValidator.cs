using Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

namespace Inventorization.Goods.BL.Validators;

/// <summary>
/// Validates CreatePurchaseOrderItemDTO
/// </summary>
public class CreatePurchaseOrderItemValidator : IValidator<CreatePurchaseOrderItemDTO>
{
    public Task<ValidationResult> ValidateAsync(CreatePurchaseOrderItemDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Task.FromResult(ValidationResult.WithErrors("DTO cannot be null"));
        
        var errors = new List<string>();
        
        if (dto.PurchaseOrderId == Guid.Empty)
            errors.Add("Purchase order ID is required");
        
        if (dto.GoodId == Guid.Empty)
            errors.Add("Good ID is required");
        
        if (dto.Quantity <= 0)
            errors.Add("Quantity must be at least 1");
        
        if (dto.UnitPrice < 0)
            errors.Add("Unit price must be non-negative");
        
        if (!string.IsNullOrEmpty(dto.Notes) && dto.Notes.Length > 1000)
            errors.Add("Notes cannot exceed 1000 characters");
        
        var result = errors.Any() 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok();
        
        return Task.FromResult(result);
    }
}
