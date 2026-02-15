namespace Inventorization.Goods.DTO.DTO.StockLocation;

public record InitStockLocationDTO(Guid Id, Guid WarehouseId, string Code) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitStockLocationDTO() : this(Guid.Empty, Guid.Empty, default!)
    {
    }
}
