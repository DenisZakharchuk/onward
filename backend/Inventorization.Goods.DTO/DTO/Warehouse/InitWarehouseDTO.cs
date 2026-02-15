namespace Inventorization.Goods.DTO.DTO.Warehouse;

public record InitWarehouseDTO(Guid Id, string Name, string Code) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitWarehouseDTO() : this(Guid.Empty, default!, default!)
    {
    }
}
