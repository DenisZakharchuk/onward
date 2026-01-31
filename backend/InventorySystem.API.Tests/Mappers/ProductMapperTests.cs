using Moq;
using InventorySystem.Business.Mappers;
using InventorySystem.DataAccess.Models;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DTOs.DTO.Product;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.API.Tests.Mappers;

[TestClass]
public class ProductMapperTests
{
    private ProductMapper _mapper = null!;
    private Mock<InventorySystem.DataAccess.Abstractions.IUnitOfWork> _unitOfWorkMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<InventorySystem.DataAccess.Abstractions.IUnitOfWork>();
        _mapper = new ProductMapper(_unitOfWorkMock.Object);
    }

    [TestMethod]
    public void Map_WithValidProduct_ReturnsCorrectDTO()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            CurrentStock = 50,
            MinimumStock = 10,
            CategoryId = categoryId
        };
        var category = new Category { Id = categoryId, Name = "Test Category" };

        _unitOfWorkMock
            .Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = _mapper.Map(product);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(product.Id, result.Id);
        Assert.AreEqual(product.Name, result.Name);
        Assert.AreEqual(product.Description, result.Description);
        Assert.AreEqual(product.Price, result.Price);
        Assert.AreEqual(product.CurrentStock, result.CurrentStock);
        Assert.AreEqual(product.MinimumStock, result.MinimumStock);
    }

    [TestMethod]
    public void Map_WithProductHavingNullDescription_ReturnsDTO()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = null,
            Price = 50m,
            CurrentStock = 25,
            MinimumStock = 5,
            CategoryId = categoryId
        };
        var category = new Category { Id = categoryId, Name = "Test Category" };

        _unitOfWorkMock
            .Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = _mapper.Map(product);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.Description);
    }

    [TestMethod]
    public void GetProjection_ReturnsValidExpression()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            CurrentStock = 50,
            MinimumStock = 10,
            CategoryId = categoryId
        };
        var projection = _mapper.GetProjection();
        var products = new List<Product> { product }.AsQueryable();

        // Act
        var result = products.Select(projection).FirstOrDefault();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(product.Id, result.Id);
        Assert.AreEqual(product.Name, result.Name);
        Assert.AreEqual(product.Price, result.Price);
    }

    [TestMethod]
    public void Map_WithDifferentPrices_MapsCorrectly()
    {
        // Arrange
        var prices = new[] { 0.01m, 1000m, 99.99m, 1m };
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Test Category" };

        _unitOfWorkMock
            .Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert
        foreach (var price in prices)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product",
                Price = price,
                CurrentStock = 10,
                MinimumStock = 1,
                CategoryId = categoryId
            };

            var result = _mapper.Map(product);

            Assert.AreEqual(price, result.Price);
        }
    }
}

[TestClass]
public class CategoryMapperTests
{
    private CategoryMapper _mapper = null!;

    [TestInitialize]
    public void Setup()
    {
        _mapper = new CategoryMapper();
    }

    [TestMethod]
    public void Map_WithValidCategory_ReturnsCorrectDTO()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Description = "Electronic devices"
        };

        // Act
        var result = _mapper.Map(category);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(category.Id, result.Id);
        Assert.AreEqual(category.Name, result.Name);
        Assert.AreEqual(category.Description, result.Description);
    }

    [TestMethod]
    public void Map_WithCategoryHavingNullDescription_ReturnsDTO()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Description = null
        };

        // Act
        var result = _mapper.Map(category);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(category.Name, result.Name);
        Assert.IsNull(result.Description);
    }

    [TestMethod]
    public void GetProjection_ReturnsValidExpression()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Description = "Electronic devices"
        };
        var projection = _mapper.GetProjection();
        var categories = new List<Category> { category }.AsQueryable();

        // Act
        var result = categories.Select(projection).FirstOrDefault();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(category.Id, result.Id);
        Assert.AreEqual(category.Name, result.Name);
    }

    [TestMethod]
    public void Map_PreservesAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Category";
        var description = "Test Description";
        var category = new Category
        {
            Id = id,
            Name = name,
            Description = description
        };

        // Act
        var result = _mapper.Map(category);

        // Assert
        Assert.AreEqual(id, result.Id);
        Assert.AreEqual(name, result.Name);
        Assert.AreEqual(description, result.Description);
    }
}
