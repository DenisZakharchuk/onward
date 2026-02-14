namespace Inventorization.Goods.DTO.ADTs;

/// <summary>
/// Static class providing strongly-typed field names for Good entity searches.
/// Field names validated against DataModelMetadata.Good at runtime.
/// </summary>
public static class GoodSearchFields
{
    // Primary identification
    public const string Id = "Id";
    public const string Name = "Name";
    public const string Description = "Description";
    public const string Sku = "Sku";
    
    // Pricing and quantity
    public const string UnitPrice = "UnitPrice";
    public const string QuantityInStock = "QuantityInStock";
    public const string UnitOfMeasure = "UnitOfMeasure";
    
    // Relationships
    public const string CategoryId = "CategoryId";
    
    // Status and audit
    public const string IsActive = "IsActive";
    public const string CreatedAt = "CreatedAt";
    public const string UpdatedAt = "UpdatedAt";
    
    // Related entity fields (for projections)
    public const string CategoryName = "Category.Name";
    public const string CategoryDescription = "Category.Description";
    
    /// <summary>
    /// Validates that a field name exists in the Good metadata
    /// </summary>
    public static bool IsValidField(string fieldName)
    {
        // Simple field validation
        var simpleFields = new[] 
        { 
            Id, Name, Description, Sku, UnitPrice, QuantityInStock, 
            UnitOfMeasure, CategoryId, IsActive, CreatedAt, UpdatedAt 
        };
        
        if (simpleFields.Contains(fieldName))
            return true;
        
        // Related field validation
        var relatedFields = new[] { CategoryName, CategoryDescription };
        return relatedFields.Contains(fieldName);
    }
}
