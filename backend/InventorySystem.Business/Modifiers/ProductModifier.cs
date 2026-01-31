using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;

namespace InventorySystem.Business.Modifiers;

/// <summary>
/// Updates Product entities from UpdateProductDTO
/// </summary>
public class ProductModifier : IEntityModifier<Product, UpdateProductDTO>
{
    public void Modify(Product entity, UpdateProductDTO dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.SKU = dto.SKU;
        entity.Price = dto.Price;
        entity.CategoryId = dto.CategoryId;
        entity.MinimumStock = dto.MinimumStock;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
