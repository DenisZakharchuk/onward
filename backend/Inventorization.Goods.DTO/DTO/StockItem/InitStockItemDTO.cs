namespace Inventorization.Goods.DTO.DTO.StockItem;

public record InitStockItemDTO(Guid Id, Guid GoodId, Guid StockLocationId, int Quantity) : Onward.Base.DTOs.InitDTO(Id)
{
    public InitStockItemDTO() : this(Guid.Empty, Guid.Empty, Guid.Empty, 0)
    {
    }
}
