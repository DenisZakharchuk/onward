namespace Inventorization.Goods.DTO.DTO.PurchaseOrder;

/// <summary>
/// DTO for updating an existing PurchaseOrder entity
/// </summary>
public class UpdatePurchaseOrderDTO : UpdateDTO
{
    [Required(ErrorMessage = "Order number is required")]
    [StringLength(100, ErrorMessage = "Order number cannot exceed 100 characters")]
    public string OrderNumber { get; set; } = null!;
    
    [Required(ErrorMessage = "Order date is required")]
    public DateTime OrderDate { get; set; }
    
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}
