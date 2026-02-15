using InventorySystem.Business.Abstractions.Services;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.API.Base.Controllers;
using Inventorization.Base.DTOs;

namespace InventorySystem.API.Controllers.Data;

/// <summary>
/// Products CRUD controller using generic DataController base.
/// All CRUD logic is inherited from DataController.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : DataController<Product, CreateProductDTO, UpdateProductDTO, DeleteProductDTO, InitProductDTO, ProductDetailsDTO, ProductSearchDTO, IProductService>
{
    public ProductsController(
        IProductService productService,
        ILogger<ServiceController> logger)
        : base(productService, logger)
    {
    }
}
