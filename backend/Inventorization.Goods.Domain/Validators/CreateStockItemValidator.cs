using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.Domain.Validators;

/// <summary>
/// Validates CreateStockItemDTO
/// </summary>
public class CreateStockItemValidator : IValidator<CreateStockItemDTO>
{
    public Task<ValidationResult> ValidateAsync(CreateStockItemDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Task.FromResult(ValidationResult.WithErrors("DTO cannot be null"));
        
        var errors = new List<string>();
        
        if (dto.GoodId == Guid.Empty)
            errors.Add("Good ID is required");
        
        if (dto.StockLocationId == Guid.Empty)
            errors.Add("Stock location ID is required");
        
        if (dto.Quantity < 0)
            errors.Add("Quantity must be non-negative");
        
        if (!string.IsNullOrEmpty(dto.BatchNumber) && dto.BatchNumber.Length > 50)
            errors.Add("Batch number cannot exceed 50 characters");
        
        if (!string.IsNullOrEmpty(dto.SerialNumber) && dto.SerialNumber.Length > 100)
            errors.Add("Serial number cannot exceed 100 characters");
        
        var result = errors.Any() 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok();
        
        return Task.FromResult(result);
    }
}
