 namespace Inventorization.Goods.DTO.DTO.Supplier;

public record InitSupplierDTO(Guid Id, string Name, string ContactEmail) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitSupplierDTO() : this(Guid.Empty, default!, default!)
    {
    }
}
