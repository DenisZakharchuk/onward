namespace Inventorization.Goods.DTO.DTO.GoodSupplier;

/// <summary>
/// DTO for searching GoodSupplier relationships
/// </summary>
public class GoodSupplierSearchDTO : SearchDTO
{
    public Guid? GoodId { get; set; }
    public Guid? SupplierId { get; set; }
    public bool? IsPreferred { get; set; }
    public decimal? MinSupplierPrice { get; set; }
    public decimal? MaxSupplierPrice { get; set; }
    public PageDTO Page { get; set; } = new();
}
