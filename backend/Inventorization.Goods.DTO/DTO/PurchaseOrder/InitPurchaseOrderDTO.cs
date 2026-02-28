namespace Inventorization.Goods.DTO.DTO.PurchaseOrder;

public record InitPurchaseOrderDTO(Guid Id, string OrderNumber, Guid SupplierId, DateTime OrderDate) : Onward.Base.DTOs.InitDTO(Id)
{
    public InitPurchaseOrderDTO() : this(Guid.Empty, default!, Guid.Empty, DateTime.MinValue)
    {
    }
}
