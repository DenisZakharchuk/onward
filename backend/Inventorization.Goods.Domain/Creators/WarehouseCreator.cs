using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Warehouse;

namespace Inventorization.Goods.Domain.Creators;

public class WarehouseCreator : IEntityCreator<Warehouse, CreateWarehouseDTO>
{
    public Warehouse Create(CreateWarehouseDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var warehouse = new Warehouse(name: dto.Name, code: dto.Code);
        
        warehouse.Update(
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
        
        return warehouse;
    }
}
