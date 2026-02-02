using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Good;

namespace Inventorization.Goods.Domain.Creators;

/// <summary>
/// Creates Good entities from CreateGoodDTO
/// </summary>
public class GoodCreator : IEntityCreator<Good, CreateGoodDTO>
{
    public Good Create(CreateGoodDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var good = new Good(
            name: dto.Name,
            sku: dto.Sku,
            unitPrice: dto.UnitPrice,
            quantityInStock: dto.QuantityInStock
        );
        
        // Use reflection to set optional Description and UnitOfMeasure
        // since constructor doesn't include them
        var descriptionProp = typeof(Good).GetProperty("Description", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var unitOfMeasureProp = typeof(Good).GetProperty("UnitOfMeasure", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (descriptionProp != null && descriptionProp.CanWrite)
        {
            descriptionProp.SetValue(good, dto.Description);
        }
        
        if (unitOfMeasureProp != null && unitOfMeasureProp.CanWrite)
        {
            unitOfMeasureProp.SetValue(good, dto.UnitOfMeasure);
        }
        
        return good;
    }
}
