using System.Linq.Expressions;
using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;

namespace InventorySystem.Business.Mappers;

/// <summary>
/// Maps Product entities to ProductDetailsDTO
/// </summary>
public class ProductMapper : IMapper<Product, ProductDetailsDTO>
{
    private readonly InventorySystem.DataAccess.Abstractions.IUnitOfWork _unitOfWork;

    public ProductMapper(InventorySystem.DataAccess.Abstractions.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public ProductDetailsDTO Map(Product entity)
    {
        var category = _unitOfWork.Categories.GetByIdAsync(entity.CategoryId).Result;
        return new ProductDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SKU = entity.SKU,
            Price = entity.Price,
            CategoryId = entity.CategoryId,
            CategoryName = category?.Name ?? string.Empty,
            CurrentStock = entity.CurrentStock,
            MinimumStock = entity.MinimumStock,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public Expression<Func<Product, ProductDetailsDTO>> GetProjection()
    {
        return p => new ProductDetailsDTO
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            SKU = p.SKU,
            Price = p.Price,
            CategoryId = p.CategoryId,
            CategoryName = p.Category != null ? p.Category.Name : string.Empty,
            CurrentStock = p.CurrentStock,
            MinimumStock = p.MinimumStock,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
