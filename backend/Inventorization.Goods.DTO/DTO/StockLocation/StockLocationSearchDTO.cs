namespace Inventorization.Goods.DTO.DTO.StockLocation;

/// <summary>
/// DTO for searching StockLocation entities
/// </summary>
public class StockLocationSearchDTO : SearchDTO
{
    public Guid? WarehouseId { get; set; }
    public string? Code { get; set; }
    public string? Aisle { get; set; }
    public bool? IsActive { get; set; }
    public PageDTO Page { get; set; } = new();
}
