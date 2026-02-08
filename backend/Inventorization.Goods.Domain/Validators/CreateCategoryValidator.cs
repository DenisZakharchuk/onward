using Inventorization.Base.Abstractions;
using Inventorization.Goods.DTO.DTO.Category;

namespace Inventorization.Goods.Domain.Validators;

/// <summary>
/// Validator for CreateCategoryDTO
/// </summary>
public class CreateCategoryValidator : IValidator<CreateCategoryDTO>
{
    public Task<ValidationResult> ValidateAsync(CreateCategoryDTO obj, CancellationToken cancellationToken = default)
    {
        if (obj == null)
            return Task.FromResult(ValidationResult.WithErrors("DTO cannot be null"));
        
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(obj.Name))
            errors.Add("Name is required");
        else if (obj.Name.Length > 200)
            errors.Add("Name cannot exceed 200 characters");
        
        if (!string.IsNullOrWhiteSpace(obj.Description) && obj.Description.Length > 1000)
            errors.Add("Description cannot exceed 1000 characters");
        
        if (obj.ParentCategoryId.HasValue && obj.ParentCategoryId.Value == Guid.Empty)
            errors.Add("Parent category ID must be a valid GUID");
        
        return Task.FromResult(errors.Count > 0 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok());
    }
}
