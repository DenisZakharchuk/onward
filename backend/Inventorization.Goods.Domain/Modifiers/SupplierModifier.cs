using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Supplier;

namespace Inventorization.Goods.Domain.Modifiers;

/// <summary>
/// Updates Supplier entities from UpdateSupplierDTO
/// </summary>
public class SupplierModifier : IEntityModifier<Supplier, UpdateSupplierDTO>
{
    public void Modify(Supplier entity, UpdateSupplierDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        entity.Update(
            name: dto.Name,
            description: dto.Description,
            contactEmail: dto.ContactEmail,
            contactPhone: dto.ContactPhone,
            address: dto.Address,
            city: dto.City,
            country: dto.Country,
            postalCode: dto.PostalCode
        );
    }
}
