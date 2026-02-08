using Inventorization.Goods.DTO.DTO.GoodSupplier;

namespace Inventorization.Goods.Domain.Validators;

/// <summary>
/// Validates CreateGoodSupplierDTO
/// </summary>
public class CreateGoodSupplierValidator : IValidator<CreateGoodSupplierDTO>
{
    public Task<ValidationResult> ValidateAsync(CreateGoodSupplierDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Task.FromResult(ValidationResult.WithErrors("DTO cannot be null"));
        
        var errors = new List<string>();
        
        if (dto.GoodId == Guid.Empty)
            errors.Add("Good ID is required");
        
        if (dto.SupplierId == Guid.Empty)
            errors.Add("Supplier ID is required");
        
        if (dto.SupplierPrice < 0)
            errors.Add("Supplier price must be non-negative");
        
        if (dto.LeadTimeDays < 0)
            errors.Add("Lead time days must be non-negative");
        
        var result = errors.Any() 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok();
        
        return Task.FromResult(result);
    }
}
