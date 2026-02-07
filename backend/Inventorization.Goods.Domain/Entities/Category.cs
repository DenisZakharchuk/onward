using Inventorization.Base.Models;

namespace Inventorization.Goods.Domain.Entities;

/// <summary>
/// Represents a category for organizing goods in the inventory system.
/// Follows entity immutability pattern with private setters and state mutation methods.
/// </summary>
public class Category : BaseEntity
{    /// <summary>
    /// Metadata for this entity - single source of truth for structure and validation
    /// </summary>
    private static readonly IDataModelMetadata<Category> Metadata = DataModelMetadata.Category;
        // Private parameterless constructor for EF Core
    private Category() { }
    
    /// <summary>
    /// Creates a new Category entity with required properties
    /// </summary>
    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required", nameof(name));
        
        Id = Guid.NewGuid();
        Name = name;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public Category? ParentCategory { get; private set; }
    public ICollection<Category> SubCategories { get; } = new List<Category>();
    public ICollection<Good> Goods { get; } = new List<Good>();
    
    /// <summary>
    /// Updates the Category's information
    /// </summary>
    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required", nameof(name));
        
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Sets the parent category
    /// </summary>
    public void SetParentCategory(Guid? parentCategoryId)
    {
        if (parentCategoryId == Id)
            throw new InvalidOperationException("Category cannot be its own parent");
        
        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Deactivates the Category (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Activates the Category
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
