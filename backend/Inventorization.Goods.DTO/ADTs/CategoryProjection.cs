namespace Inventorization.Goods.DTO.ADTs;

/// <summary>
/// Flexible projection DTO for Category in search results.
/// Contains only the fields requested in the projection.
/// </summary>
public class CategoryProjection
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Recursive parent category (if needed)
    public CategoryProjection? ParentCategory { get; set; }
}
