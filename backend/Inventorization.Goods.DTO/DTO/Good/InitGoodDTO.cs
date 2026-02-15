namespace Inventorization.Goods.DTO.DTO.Good;

public record InitGoodDTO(Guid Id, string Name, string Sku, decimal UnitPrice, int QuantityInStock) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitGoodDTO() : this(Guid.Empty, default!, default!, 0, 0)
    {
    }
}
