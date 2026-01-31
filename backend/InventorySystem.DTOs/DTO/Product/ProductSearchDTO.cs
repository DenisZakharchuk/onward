using Inventorization.Base.DTOs;

namespace InventorySystem.DTOs.DTO.Product;

/// <summary>
/// Product Search DTO
/// </summary>
public class ProductSearchDTO : SearchDTO
{
    public string? NameFilter { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? LowStockOnly { get; set; }
}
