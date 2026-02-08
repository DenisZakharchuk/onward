namespace Inventorization.Goods.DTO.DTO.GoodSupplier;

/// <summary>
/// DTO for GoodSupplier relationship details response
/// </summary>
public class GoodSupplierDetailsDTO : DetailsDTO
{
    public Guid GoodId { get; set; }
    public Guid SupplierId { get; set; }
    public decimal SupplierPrice { get; set; }
    public int LeadTimeDays { get; set; }
    public bool IsPreferred { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
