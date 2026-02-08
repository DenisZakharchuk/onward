using Inventorization.Goods.Common.Enums;

namespace Inventorization.Goods.DTO.DTO.PurchaseOrder;

/// <summary>
/// DTO for searching PurchaseOrder entities
/// </summary>
public class PurchaseOrderSearchDTO : SearchDTO
{
    public string? OrderNumber { get; set; }
    public Guid? SupplierId { get; set; }
    public PurchaseOrderStatus? Status { get; set; }
    public DateTime? OrderDateFrom { get; set; }
    public DateTime? OrderDateTo { get; set; }
    public DateTime? ExpectedDeliveryDateFrom { get; set; }
    public DateTime? ExpectedDeliveryDateTo { get; set; }
    public PageDTO Page { get; set; } = new();
}
