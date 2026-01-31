using InventorySystem.Business.Creators;
using InventorySystem.DTOs.DTO.Product;
using InventorySystem.DTOs.DTO.Category;
using InventorySystem.DataAccess.Models;

namespace InventorySystem.API.Tests.Creators;

[TestClass]
public class ProductCreatorTests
{
    private ProductCreator _creator = null!;

    [TestInitialize]
    public void Setup()
    {
        _creator = new ProductCreator();
    }

    [TestMethod]
    public void Create_WithValidDTO_CreatesProductCorrectly()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new CreateProductDTO
        {
            Name = "New Product",
            Description = "Product Description",
            Price = 99.99m,
            CategoryId = categoryId,
            InitialStock = 50,
            MinimumStock = 10
        };

        // Act
        var product = _creator.Create(dto);

        // Assert
        Assert.IsNotNull(product);
        Assert.AreNotEqual(Guid.Empty, product.Id);
        Assert.AreEqual(dto.Name, product.Name);
        Assert.AreEqual(dto.Description, product.Description);
        Assert.AreEqual(dto.Price, product.Price);
        Assert.AreEqual(dto.CategoryId, product.CategoryId);
        Assert.AreEqual(dto.InitialStock, product.CurrentStock);
        Assert.AreEqual(dto.MinimumStock, product.MinimumStock);
    }

    [TestMethod]
    public void Create_GeneratesUniqueId()
    {
        // Arrange
        var dto = new CreateProductDTO
        {
            Name = "Product",
            Price = 50m,
            CategoryId = Guid.NewGuid(),
            InitialStock = 10,
            MinimumStock = 2
        };

        // Act
        var product1 = _creator.Create(dto);
        var product2 = _creator.Create(dto);

        // Assert
        Assert.AreNotEqual(product1.Id, product2.Id);
    }

    [TestMethod]
    public void Create_WithoutDescription_CreatesProduct()
    {
        // Arrange
        var dto = new CreateProductDTO
        {
            Name = "Product",
            Description = null,
            Price = 50m,
            CategoryId = Guid.NewGuid(),
            InitialStock = 10,
            MinimumStock = 2
        };

        // Act
        var product = _creator.Create(dto);

        // Assert
        Assert.IsNotNull(product);
        Assert.AreEqual(dto.Name, product.Name);
        Assert.IsNull(product.Description);
    }

    [TestMethod]
    public void Create_InitializesProductProperties()
    {
        // Arrange
        var dto = new CreateProductDTO
        {
            Name = "Test Product",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            InitialStock = 100,
            MinimumStock = 20
        };

        // Act
        var product = _creator.Create(dto);

        // Assert
        Assert.IsNotNull(product);
        Assert.AreEqual(dto.InitialStock, product.CurrentStock);
        Assert.AreEqual(dto.MinimumStock, product.MinimumStock);
        Assert.AreEqual(dto.Price, product.Price);
    }
}

[TestClass]
public class CategoryCreatorTests
{
    private CategoryCreator _creator = null!;

    [TestInitialize]
    public void Setup()
    {
        _creator = new CategoryCreator();
    }

    [TestMethod]
    public void Create_WithValidDTO_CreatesCategoryCorrectly()
    {
        // Arrange
        var dto = new CreateCategoryDTO
        {
            Name = "Electronics",
            Description = "Electronic devices and accessories"
        };

        // Act
        var category = _creator.Create(dto);

        // Assert
        Assert.IsNotNull(category);
        Assert.AreNotEqual(Guid.Empty, category.Id);
        Assert.AreEqual(dto.Name, category.Name);
        Assert.AreEqual(dto.Description, category.Description);
    }

    [TestMethod]
    public void Create_GeneratesUniqueId()
    {
        // Arrange
        var dto = new CreateCategoryDTO
        {
            Name = "Category",
            Description = "Description"
        };

        // Act
        var category1 = _creator.Create(dto);
        var category2 = _creator.Create(dto);

        // Assert
        Assert.AreNotEqual(category1.Id, category2.Id);
    }

    [TestMethod]
    public void Create_WithoutDescription_CreatesCategory()
    {
        // Arrange
        var dto = new CreateCategoryDTO
        {
            Name = "Category",
            Description = null
        };

        // Act
        var category = _creator.Create(dto);

        // Assert
        Assert.IsNotNull(category);
        Assert.AreEqual(dto.Name, category.Name);
        Assert.IsNull(category.Description);
    }

    [TestMethod]
    public void Create_PreservesAllProperties()
    {
        // Arrange
        var name = "Test Category";
        var description = "Test Description";
        var dto = new CreateCategoryDTO
        {
            Name = name,
            Description = description
        };

        // Act
        var category = _creator.Create(dto);

        // Assert
        Assert.AreEqual(name, category.Name);
        Assert.AreEqual(description, category.Description);
    }
}
