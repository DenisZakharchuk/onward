namespace Inventorization.Goods.DTO.DTO.Category;

/// <summary>
/// DTO for updating an existing Category entity
/// </summary>
public class UpdateCategoryDTO : UpdateDTO
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = null!;
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
    
    public Guid? ParentCategoryId { get; set; }
}
