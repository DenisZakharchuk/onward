using InventorySystem.Business.SearchProviders;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.API.Tests.SearchProviders;

[TestClass]
public class ProductSearchProviderTests
{
    private ProductSearchProvider _provider = null!;

    [TestInitialize]
    public void Setup()
    {
        _provider = new ProductSearchProvider();
    }

    [TestMethod]
    public void GetSearchExpression_WithNoFilter_ReturnsAllProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDTO();
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 50m },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 75m }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = products.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetSearchExpression_WithNameFilter_ReturnsMatchingProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDTO { NameFilter = "Product 1" };
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 50m },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 75m }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = products.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Product 1", result[0].Name);
    }

    [TestMethod]
    public void GetSearchExpression_WithMinPriceFilter_ReturnsProductsAbovePrice()
    {
        // Arrange
        var searchDto = new ProductSearchDTO { MinPrice = 60m };
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 50m },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 75m }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = products.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(75m, result[0].Price);
    }

    [TestMethod]
    public void GetSearchExpression_WithMaxPriceFilter_ReturnsProductsBelowPrice()
    {
        // Arrange
        var searchDto = new ProductSearchDTO { MaxPrice = 60m };
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 50m },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 75m }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = products.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(50m, result[0].Price);
    }

    [TestMethod]
    public void GetSearchExpression_WithMultipleFilters_ReturnsFiltered()
    {
        // Arrange
        var searchDto = new ProductSearchDTO
        {
            NameFilter = "Product",
            MinPrice = 50m,
            MaxPrice = 75m
        };
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 40m },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 60m },
            new Product { Id = Guid.NewGuid(), Name = "Other", Price = 60m }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = products.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Product 2", result[0].Name);
    }
}

[TestClass]
public class CategorySearchProviderTests
{
    private CategorySearchProvider _provider = null!;

    [TestInitialize]
    public void Setup()
    {
        _provider = new CategorySearchProvider();
    }

    [TestMethod]
    public void GetSearchExpression_WithNoFilter_ReturnsAllCategories()
    {
        // Arrange
        var searchDto = new CategorySearchDTO();
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Electronics" },
            new Category { Id = Guid.NewGuid(), Name = "Books" }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = categories.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetSearchExpression_WithNameFilter_ReturnsMatchingCategories()
    {
        // Arrange
        var searchDto = new CategorySearchDTO { NameFilter = "Electronics" };
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Electronics" },
            new Category { Id = Guid.NewGuid(), Name = "Books" }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = categories.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Electronics", result[0].Name);
    }
}
