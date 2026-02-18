using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.Warehouse;

namespace Inventorization.Goods.BL.Mappers;

public class WarehouseMapper : IMapper<Warehouse, WarehouseDetailsDTO>
{
    public WarehouseDetailsDTO Map(Warehouse entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        return new WarehouseDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Description = entity.Description,
            Address = entity.Address,
            City = entity.City,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            ManagerName = entity.ManagerName,
            ContactPhone = entity.ContactPhone,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    public Expression<Func<Warehouse, WarehouseDetailsDTO>> GetProjection()
    {
        return entity => new WarehouseDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Description = entity.Description,
            Address = entity.Address,
            City = entity.City,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            ManagerName = entity.ManagerName,
            ContactPhone = entity.ContactPhone,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
