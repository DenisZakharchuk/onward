namespace Inventorization.Goods.DTO.DTO.GoodSupplier;

/// <summary>
/// DTO for creating a new GoodSupplier relationship
/// </summary>
public class CreateGoodSupplierDTO : CreateDTO
{
    [Required(ErrorMessage = "Good ID is required")]
    public Guid GoodId { get; set; }
    
    [Required(ErrorMessage = "Supplier ID is required")]
    public Guid SupplierId { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Supplier price must be non-negative")]
    public decimal SupplierPrice { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Lead time days must be non-negative")]
    public int LeadTimeDays { get; set; }
    
    public bool IsPreferred { get; set; }
}
