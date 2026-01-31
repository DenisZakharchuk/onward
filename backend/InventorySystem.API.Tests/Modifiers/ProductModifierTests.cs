using InventorySystem.Business.Modifiers;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.API.Tests.Modifiers;

[TestClass]
public class ProductModifierTests
{
    private ProductModifier _modifier = null!;

    [TestInitialize]
    public void Setup()
    {
        _modifier = new ProductModifier();
    }

    [TestMethod]
    public void Modify_UpdatesProductProperties()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            Description = "Old Description",
            Price = 50m,
            MinimumStock = 5
        };
        var updateDto = new UpdateProductDTO
        {
            Id = product.Id,
            Name = "New Name",
            Description = "New Description",
            Price = 75m,
            MinimumStock = 10
        };

        // Act
        _modifier.Modify(product, updateDto);

        // Assert
        Assert.AreEqual("New Name", product.Name);
        Assert.AreEqual("New Description", product.Description);
        Assert.AreEqual(75m, product.Price);
        Assert.AreEqual(10, product.MinimumStock);
    }

    [TestMethod]
    public void Modify_PreservesId()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Product" };
        var originalId = product.Id;
        var updateDto = new UpdateProductDTO
        {
            Id = product.Id,
            Name = "Updated Product"
        };

        // Act
        _modifier.Modify(product, updateDto);

        // Assert
        Assert.AreEqual(originalId, product.Id);
    }

    [TestMethod]
    public void Modify_WithNullDescription_UpdatesDescription()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Description = "Old Description"
        };
        var updateDto = new UpdateProductDTO
        {
            Id = product.Id,
            Name = "Product",
            Description = null
        };

        // Act
        _modifier.Modify(product, updateDto);

        // Assert
        Assert.IsNull(product.Description);
    }
}

[TestClass]
public class CategoryModifierTests
{
    private CategoryModifier _modifier = null!;

    [TestInitialize]
    public void Setup()
    {
        _modifier = new CategoryModifier();
    }

    [TestMethod]
    public void Modify_UpdatesCategoryProperties()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            Description = "Old Description"
        };
        var updateDto = new UpdateCategoryDTO
        {
            Id = category.Id,
            Name = "New Name",
            Description = "New Description"
        };

        // Act
        _modifier.Modify(category, updateDto);

        // Assert
        Assert.AreEqual("New Name", category.Name);
        Assert.AreEqual("New Description", category.Description);
    }

    [TestMethod]
    public void Modify_PreservesId()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Category" };
        var originalId = category.Id;
        var updateDto = new UpdateCategoryDTO
        {
            Id = category.Id,
            Name = "Updated"
        };

        // Act
        _modifier.Modify(category, updateDto);

        // Assert
        Assert.AreEqual(originalId, category.Id);
    }

    [TestMethod]
    public void Modify_WithNullDescription_UpdatesDescription()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Description = "Old"
        };
        var updateDto = new UpdateCategoryDTO
        {
            Id = category.Id,
            Name = "Category",
            Description = null
        };

        // Act
        _modifier.Modify(category, updateDto);

        // Assert
        Assert.IsNull(category.Description);
    }
}
