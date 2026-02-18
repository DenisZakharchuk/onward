using Inventorization.Goods.DTO.DTO.PurchaseOrder;

namespace Inventorization.Goods.BL.Validators;

/// <summary>
/// Validates CreatePurchaseOrderDTO
/// </summary>
public class CreatePurchaseOrderValidator : IValidator<CreatePurchaseOrderDTO>
{
    public Task<ValidationResult> ValidateAsync(CreatePurchaseOrderDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Task.FromResult(ValidationResult.WithErrors("DTO cannot be null"));
        
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(dto.OrderNumber))
            errors.Add("Order number is required");
        else if (dto.OrderNumber.Length > 100)
            errors.Add("Order number cannot exceed 100 characters");
        
        if (dto.SupplierId == Guid.Empty)
            errors.Add("Supplier ID is required");
        
        if (dto.OrderDate == default)
            errors.Add("Order date is required");
        
        if (dto.ExpectedDeliveryDate.HasValue && dto.ExpectedDeliveryDate.Value < dto.OrderDate)
            errors.Add("Expected delivery date cannot be before order date");
        
        if (!string.IsNullOrEmpty(dto.Notes) && dto.Notes.Length > 2000)
            errors.Add("Notes cannot exceed 2000 characters");
        
        var result = errors.Any() 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok();
        
        return Task.FromResult(result);
    }
}
