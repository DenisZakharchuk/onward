using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.Category;

/// <summary>
/// Category Search DTO
/// </summary>
public class CategorySearchDTO : SearchDTO
{
    public string? NameFilter { get; set; }
}
