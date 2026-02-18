using Inventorization.Base.Abstractions;
using Inventorization.Goods.DTO.DTO.Supplier;
using System.Text.RegularExpressions;

namespace Inventorization.Goods.BL.Validators;

/// <summary>
/// Validator for UpdateSupplierDTO
/// </summary>
public class UpdateSupplierValidator : IValidator<UpdateSupplierDTO>
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public Task<ValidationResult> ValidateAsync(UpdateSupplierDTO obj, CancellationToken cancellationToken = default)
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
        
        if (string.IsNullOrWhiteSpace(obj.ContactEmail))
            errors.Add("Contact email is required");
        else if (!EmailRegex.IsMatch(obj.ContactEmail))
            errors.Add("Invalid email format");
        else if (obj.ContactEmail.Length > 100)
            errors.Add("Contact email cannot exceed 100 characters");
        
        if (!string.IsNullOrWhiteSpace(obj.ContactPhone) && obj.ContactPhone.Length > 20)
            errors.Add("Contact phone cannot exceed 20 characters");
        
        if (!string.IsNullOrWhiteSpace(obj.Address) && obj.Address.Length > 500)
            errors.Add("Address cannot exceed 500 characters");
        
        if (!string.IsNullOrWhiteSpace(obj.City) && obj.City.Length > 100)
            errors.Add("City cannot exceed 100 characters");
        
        if (!string.IsNullOrWhiteSpace(obj.Country) && obj.Country.Length > 100)
            errors.Add("Country cannot exceed 100 characters");
        
        if (!string.IsNullOrWhiteSpace(obj.PostalCode) && obj.PostalCode.Length > 20)
            errors.Add("Postal code cannot exceed 20 characters");
        
        return Task.FromResult(errors.Count > 0 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok());
    }
}
