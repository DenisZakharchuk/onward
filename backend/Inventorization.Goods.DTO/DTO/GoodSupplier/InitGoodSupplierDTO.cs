namespace Inventorization.Goods.DTO.DTO.GoodSupplier;

public record InitGoodSupplierDTO(
    Guid Id,
    Guid GoodId,
    Guid SupplierId,
    decimal SupplierPrice,
    int LeadTimeDays,
    bool IsPreferred
) : Inventorization.Base.DTOs.InitDTO(Id)
{
    public InitGoodSupplierDTO() : this(Guid.Empty, Guid.Empty, Guid.Empty, 0, 0, false)
    {
    }
}
