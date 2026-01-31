using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;

namespace InventorySystem.Business.Creators;

/// <summary>
/// Creates Product entities from CreateProductDTO
/// </summary>
public class ProductCreator : IEntityCreator<Product, CreateProductDTO>
{
    public Product Create(CreateProductDTO dto)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            CurrentStock = dto.InitialStock,
            MinimumStock = dto.MinimumStock,
            CreatedAt = DateTime.UtcNow
        };
    }
}
