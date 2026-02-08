using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Supplier;

namespace Inventorization.Goods.Domain.Mappers;

/// <summary>
/// Maps Supplier entities to SupplierDetailsDTO
/// </summary>
public class SupplierMapper : IMapper<Supplier, SupplierDetailsDTO>
{
    public SupplierDetailsDTO Map(Supplier entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        return new SupplierDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ContactEmail = entity.ContactEmail,
            ContactPhone = entity.ContactPhone,
            Address = entity.Address,
            City = entity.City,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    public Expression<Func<Supplier, SupplierDetailsDTO>> GetProjection()
    {
        return entity => new SupplierDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ContactEmail = entity.ContactEmail,
            ContactPhone = entity.ContactPhone,
            Address = entity.Address,
            City = entity.City,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
