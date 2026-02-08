namespace Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

/// <summary>
/// DTO for searching PurchaseOrderItem entities
/// </summary>
public class PurchaseOrderItemSearchDTO : SearchDTO
{
    public Guid? PurchaseOrderId { get; set; }
    public Guid? GoodId { get; set; }
    public bool? IsFullyReceived { get; set; }
    public PageDTO Page { get; set; } = new();
}
