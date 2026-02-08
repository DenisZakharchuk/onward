using Inventorization.Goods.Common.Enums;

namespace Inventorization.Goods.DTO.DTO.PurchaseOrder;

/// <summary>
/// DTO for PurchaseOrder entity details response
/// </summary>
public class PurchaseOrderDetailsDTO : DetailsDTO
{
    public string OrderNumber { get; set; } = null!;
    public Guid SupplierId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
