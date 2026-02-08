using Inventorization.Goods.DTO.DTO.StockLocation;

namespace Inventorization.Goods.Domain.Validators;

/// <summary>
/// Validates UpdateStockLocationDTO
/// </summary>
public class UpdateStockLocationValidator : IValidator<UpdateStockLocationDTO>
{
    public Task<ValidationResult> ValidateAsync(UpdateStockLocationDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Task.FromResult(ValidationResult.WithErrors("DTO cannot be null"));
        
        var errors = new List<string>();
        
        if (dto.Id == Guid.Empty)
            errors.Add("Id is required");
        
        if (dto.WarehouseId == Guid.Empty)
            errors.Add("Warehouse ID is required");
        
        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Code is required");
        else if (dto.Code.Length > 50)
            errors.Add("Code cannot exceed 50 characters");
        
        if (!string.IsNullOrEmpty(dto.Aisle) && dto.Aisle.Length > 50)
            errors.Add("Aisle cannot exceed 50 characters");
        
        if (!string.IsNullOrEmpty(dto.Shelf) && dto.Shelf.Length > 50)
            errors.Add("Shelf cannot exceed 50 characters");
        
        if (!string.IsNullOrEmpty(dto.Bin) && dto.Bin.Length > 50)
            errors.Add("Bin cannot exceed 50 characters");
        
        if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 500)
            errors.Add("Description cannot exceed 500 characters");
        
        var result = errors.Any() 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok();
        
        return Task.FromResult(result);
    }
}
