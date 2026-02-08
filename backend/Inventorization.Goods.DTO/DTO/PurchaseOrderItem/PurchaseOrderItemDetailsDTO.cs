namespace Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

/// <summary>
/// DTO for PurchaseOrderItem entity details response
/// </summary>
public class PurchaseOrderItemDetailsDTO : DetailsDTO
{
    public Guid PurchaseOrderId { get; set; }
    public Guid GoodId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int ReceivedQuantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Computed properties
    public decimal TotalPrice => Quantity * UnitPrice;
    public bool IsFullyReceived => ReceivedQuantity >= Quantity;
}
