namespace Inventorization.Goods.DTO.DTO.StockLocation;

/// <summary>
/// DTO for StockLocation entity details response
/// </summary>
public class StockLocationDetailsDTO : DetailsDTO
{
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = null!;
    public string? Aisle { get; set; }
    public string? Shelf { get; set; }
    public string? Bin { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
