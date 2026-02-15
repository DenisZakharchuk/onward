namespace Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

public record InitPurchaseOrderItemDTO(Guid Id, Guid PurchaseOrderId, Guid GoodId, int Quantity, decimal UnitPrice) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitPurchaseOrderItemDTO() : this(Guid.Empty, Guid.Empty, Guid.Empty, 0, 0)
    {
    }
}
