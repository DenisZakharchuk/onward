namespace Inventorization.Goods.DTO.DTO.StockItem;

/// <summary>
/// DTO for searching StockItem entities
/// </summary>
public class StockItemSearchDTO : SearchDTO
{
    public Guid? GoodId { get; set; }
    public Guid? StockLocationId { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDateFrom { get; set; }
    public DateTime? ExpiryDateTo { get; set; }
    public PageDTO Page { get; set; } = new();
}
