using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.Warehouse;

namespace Inventorization.Goods.BL.Modifiers;

public class WarehouseModifier : IEntityModifier<Warehouse, UpdateWarehouseDTO>
{
    public void Modify(Warehouse entity, UpdateWarehouseDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        entity.Update(
            name: dto.Name,
            code: dto.Code,
            description: dto.Description,
            address: dto.Address,
            city: dto.City,
            country: dto.Country,
            postalCode: dto.PostalCode,
            managerName: dto.ManagerName,
            contactPhone: dto.ContactPhone
        );
    }
}
