namespace Inventorization.Goods.DTO.DTO.Category;

/// <summary>
/// DTO for searching Category entities
/// </summary>
public class CategorySearchDTO : SearchDTO
{
    public string? Name { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
    public PageDTO Page { get; set; } = new();
}
