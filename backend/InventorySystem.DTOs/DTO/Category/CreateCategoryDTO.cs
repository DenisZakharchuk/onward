using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.Category;

/// <summary>
/// Create Category DTO
/// </summary>
public class CreateCategoryDTO : CreateDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
