using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.Category;

/// <summary>
/// Category Details DTO (returned from Get operations)
/// </summary>
public class CategoryDetailsDTO : DetailsDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
