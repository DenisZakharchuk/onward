using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.Product;

/// <summary>
/// Create Product DTO
/// </summary>
public class CreateProductDTO : CreateDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public int InitialStock { get; set; }
    public int MinimumStock { get; set; }
}
