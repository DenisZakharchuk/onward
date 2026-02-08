namespace Inventorization.Goods.DTO.DTO.Category;

/// <summary>
/// DTO for Category entity details response
/// </summary>
public class CategoryDetailsDTO : DetailsDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public CategoryDetailsDTO? ParentCategory { get; set; }
}
