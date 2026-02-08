using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.Domain.Validators;

/// <summary>
/// Validates UpdateStockItemDTO
/// </summary>
public class UpdateStockItemValidator : IValidator<UpdateStockItemDTO>
{
    public Task<ValidationResult> ValidateAsync(UpdateStockItemDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Task.FromResult(ValidationResult.WithErrors("DTO cannot be null"));
        
        var errors = new List<string>();
        
        if (dto.Id == Guid.Empty)
            errors.Add("Id is required");
        
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
