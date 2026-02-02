namespace Inventorization.Goods.DTO.DTO.Good;

/// <summary>
/// DTO for Good entity details response
/// </summary>
public class GoodDetailsDTO : DetailsDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Sku { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int QuantityInStock { get; set; }
    public string? UnitOfMeasure { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
