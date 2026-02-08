namespace Inventorization.Goods.DTO.DTO.StockItem;

/// <summary>
/// DTO for StockItem entity details response
/// </summary>
public class StockItemDetailsDTO : DetailsDTO
{
    public Guid GoodId { get; set; }
    public Guid StockLocationId { get; set; }
    public int Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
