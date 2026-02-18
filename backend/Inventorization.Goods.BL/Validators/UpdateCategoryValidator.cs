using Inventorization.Base.Abstractions;
using Inventorization.Goods.DTO.DTO.Category;

namespace Inventorization.Goods.BL.Validators;

/// <summary>
/// Validator for UpdateCategoryDTO
/// </summary>
public class UpdateCategoryValidator : IValidator<UpdateCategoryDTO>
{
    public Task<ValidationResult> ValidateAsync(UpdateCategoryDTO obj, CancellationToken cancellationToken = default)
    {
        if (obj == null)
            return Task.FromResult(ValidationResult.WithErrors("DTO cannot be null"));
        
        var errors = new List<string>();
        
        if (obj.Id == Guid.Empty)
            errors.Add("ID is required");
        
        if (string.IsNullOrWhiteSpace(obj.Name))
            errors.Add("Name is required");
        else if (obj.Name.Length > 200)
            errors.Add("Name cannot exceed 200 characters");
        
        if (!string.IsNullOrWhiteSpace(obj.Description) && obj.Description.Length > 1000)
            errors.Add("Description cannot exceed 1000 characters");
        
        if (obj.ParentCategoryId.HasValue && obj.ParentCategoryId.Value == Guid.Empty)
            errors.Add("Parent category ID must be a valid GUID");
        
        if (obj.ParentCategoryId.HasValue && obj.ParentCategoryId.Value == obj.Id)
            errors.Add("A category cannot be its own parent");
        
        return Task.FromResult(errors.Count > 0 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok());
    }
}
