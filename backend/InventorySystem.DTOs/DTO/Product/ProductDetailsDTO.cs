using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.Product;

/// <summary>
/// Product Details DTO (returned from Get operations)
/// </summary>
public class ProductDetailsDTO : DetailsDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
