using Inventorization.Goods.DTO.DTO.Good;

namespace Inventorization.Goods.Domain.Validators;

/// <summary>
/// Validates CreateGoodDTO
/// </summary>
public class CreateGoodValidator : IValidator<CreateGoodDTO>
{
    public Task<ValidationResult> ValidateAsync(CreateGoodDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Task.FromResult(ValidationResult.WithErrors("DTO cannot be null"));
        
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Name is required");
        else if (dto.Name.Length > 200)
            errors.Add("Name cannot exceed 200 characters");
        
        if (string.IsNullOrWhiteSpace(dto.Sku))
            errors.Add("SKU is required");
        else if (dto.Sku.Length > 50)
            errors.Add("SKU cannot exceed 50 characters");
        
        if (dto.UnitPrice < 0)
            errors.Add("Unit price must be non-negative");
        
        if (dto.QuantityInStock < 0)
            errors.Add("Quantity in stock must be non-negative");
        
        if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 1000)
            errors.Add("Description cannot exceed 1000 characters");
        
        if (!string.IsNullOrEmpty(dto.UnitOfMeasure) && dto.UnitOfMeasure.Length > 50)
            errors.Add("Unit of measure cannot exceed 50 characters");
        
        var result = errors.Any() 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok();
        
        return Task.FromResult(result);
    }
}
