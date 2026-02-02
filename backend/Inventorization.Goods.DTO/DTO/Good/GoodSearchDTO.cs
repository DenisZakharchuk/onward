namespace Inventorization.Goods.DTO.DTO.Good;

/// <summary>
/// DTO for searching/filtering Good entities
/// </summary>
public class GoodSearchDTO : SearchDTO
{
    public string? NameFilter { get; set; }
    public string? SkuFilter { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsActiveFilter { get; set; }
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
}
