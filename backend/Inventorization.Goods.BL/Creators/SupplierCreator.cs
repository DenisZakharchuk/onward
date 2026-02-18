using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.Supplier;

namespace Inventorization.Goods.BL.Creators;

/// <summary>
/// Creates Supplier entities from CreateSupplierDTO
/// </summary>
public class SupplierCreator : IEntityCreator<Supplier, CreateSupplierDTO>
{
    public Supplier Create(CreateSupplierDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var supplier = new Supplier(name: dto.Name, contactEmail: dto.ContactEmail);
        
        // Set optional properties via Update method
        supplier.Update(
            name: dto.Name,
            description: dto.Description,
            contactEmail: dto.ContactEmail,
            contactPhone: dto.ContactPhone,
            address: dto.Address,
            city: dto.City,
            country: dto.Country,
            postalCode: dto.PostalCode
        );
        
        return supplier;
    }
}
