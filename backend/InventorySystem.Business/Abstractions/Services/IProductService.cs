using Inventorization.Base.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;

namespace InventorySystem.Business.Abstractions.Services;

/// <summary>
/// Product data service interface
/// </summary>
public interface IProductService : IDataService<Product, CreateProductDTO, UpdateProductDTO, Inventorization.Base.DTOs.DeleteDTO, InitProductDTO, ProductDetailsDTO, ProductSearchDTO>
{
}
