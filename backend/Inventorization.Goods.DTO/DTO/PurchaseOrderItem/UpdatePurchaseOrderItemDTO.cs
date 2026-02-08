namespace Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

/// <summary>
/// DTO for updating an existing PurchaseOrderItem entity
/// </summary>
public class UpdatePurchaseOrderItemDTO : UpdateDTO
{
    [Required(ErrorMessage = "Purchase order ID is required")]
    public Guid PurchaseOrderId { get; set; }
    
    [Required(ErrorMessage = "Good ID is required")]
    public Guid GoodId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Unit price must be non-negative")]
    public decimal UnitPrice { get; set; }
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
