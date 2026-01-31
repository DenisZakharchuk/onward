using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.Category;

/// <summary>
/// Update Category DTO
/// </summary>
public class UpdateCategoryDTO : UpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
