namespace Inventorization.Goods.DTO.ADTs;

/// <summary>
/// Constants for Category search field names
/// Ensures type-safe field references in search queries
/// </summary>
public static class CategorySearchFields
{
    // Direct Category properties
    public const string Id = "Id";
    public const string Name = "Name";
    public const string Description = "Description";
    public const string ParentCategoryId = "ParentCategoryId";
    public const string IsActive = "IsActive";
    public const string CreatedAt = "CreatedAt";
    public const string UpdatedAt = "UpdatedAt";
    
    // Related entity fields (ParentCategory)
    public const string ParentCategoryName = "ParentCategory.Name";
    public const string ParentCategoryDescription = "ParentCategory.Description";
    public const string ParentCategoryIsActive = "ParentCategory.IsActive";
    
    /// <summary>
    /// All valid field names for Category search
    /// </summary>
    public static readonly IReadOnlySet<string> AllFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Id, Name, Description, ParentCategoryId, IsActive, CreatedAt, UpdatedAt,
        ParentCategoryName, ParentCategoryDescription, ParentCategoryIsActive
    };
    
    /// <summary>
    /// Fields that require joining related entities
    /// </summary>
    public static readonly IReadOnlySet<string> RelatedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ParentCategoryName, ParentCategoryDescription, ParentCategoryIsActive
    };
    
    /// <summary>
    /// Checks if a field name is valid
    /// </summary>
    public static bool IsValidField(string fieldName) => AllFields.Contains(fieldName);
    
    /// <summary>
    /// Checks if a field requires related entity join
    /// </summary>
    public static bool RequiresRelatedEntity(string fieldName) => RelatedFields.Contains(fieldName);
}
