using InventorySystem.Business.Validators;
using InventorySystem.DTOs.DTO.Product;
using InventorySystem.DTOs.DTO.Category;
using Inventorization.Base.DTOs;

namespace InventorySystem.API.Tests.Validators;

[TestClass]
public class ProductValidatorTests
{
    #region CreateProductValidator Tests

    [TestMethod]
    public async Task CreateProductValidator_WithValidDTO_ReturnsOk()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDTO
        {
            Name = "Valid Product",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            InitialStock = 10,
            MinimumStock = 2
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, result.Errors.Count);
    }

    [TestMethod]
    public async Task CreateProductValidator_WithEmptyName_ReturnsFailed()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDTO
        {
            Name = "",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            InitialStock = 10,
            MinimumStock = 2
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task CreateProductValidator_WithNegativePrice_ReturnsFailed()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDTO
        {
            Name = "Product",
            Price = -10m,
            CategoryId = Guid.NewGuid(),
            InitialStock = 10,
            MinimumStock = 2
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task CreateProductValidator_WithZeroPrice_ReturnsOk()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDTO
        {
            Name = "Product",
            Price = 0m,
            CategoryId = Guid.NewGuid(),
            InitialStock = 10,
            MinimumStock = 2
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task CreateProductValidator_WithNullCategoryId_ReturnsFailed()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDTO
        {
            Name = "Product",
            Price = 99.99m,
            CategoryId = Guid.Empty,
            InitialStock = 10,
            MinimumStock = 2
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task CreateProductValidator_WithNegativeStock_ReturnsFailed()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDTO
        {
            Name = "Product",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            InitialStock = -5,
            MinimumStock = 2
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task CreateProductValidator_WithMinimumStockGreaterThanInitial_ReturnsOk()
    {
        // Arrange - validators don't check this constraint
        var validator = new CreateProductValidator();
        var dto = new CreateProductDTO
        {
            Name = "Product",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            InitialStock = 5,
            MinimumStock = 10
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    #endregion

    #region UpdateProductValidator Tests

    [TestMethod]
    public async Task UpdateProductValidator_WithValidDTO_ReturnsOk()
    {
        // Arrange
        var validator = new UpdateProductValidator();
        var dto = new UpdateProductDTO
        {
            Id = Guid.NewGuid(),
            Name = "Updated Product",
            Price = 149.99m,
            CategoryId = Guid.NewGuid(),
            MinimumStock = 3
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task UpdateProductValidator_WithEmptyName_ReturnsFailed()
    {
        // Arrange
        var validator = new UpdateProductValidator();
        var dto = new UpdateProductDTO
        {
            Id = Guid.NewGuid(),
            Name = "",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            MinimumStock = 2
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task UpdateProductValidator_WithZeroId_ReturnsFailed()
    {
        // Arrange
        var validator = new UpdateProductValidator();
        var dto = new UpdateProductDTO
        {
            Id = Guid.Empty,
            Name = "Product",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            MinimumStock = 2
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task UpdateProductValidator_WithNegativePrice_ReturnsFailed()
    {
        // Arrange
        var validator = new UpdateProductValidator();
        var dto = new UpdateProductDTO
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Price = -50m,
            CategoryId = Guid.NewGuid(),
            MinimumStock = 2
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    #endregion
}

[TestClass]
public class CategoryValidatorTests
{
    #region CreateCategoryValidator Tests

    [TestMethod]
    public async Task CreateCategoryValidator_WithValidDTO_ReturnsOk()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var dto = new CreateCategoryDTO
        {
            Name = "Electronics",
            Description = "Electronic devices and accessories"
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task CreateCategoryValidator_WithEmptyName_ReturnsFailed()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var dto = new CreateCategoryDTO
        {
            Name = "",
            Description = "Some description"
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task CreateCategoryValidator_WithNameTooLong_ReturnsFailed()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var dto = new CreateCategoryDTO
        {
            Name = new string('a', 256),
            Description = "Description"
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task CreateCategoryValidator_WithNullDescription_ReturnsOk()
    {
        // Arrange
        var validator = new CreateCategoryValidator();
        var dto = new CreateCategoryDTO
        {
            Name = "Category",
            Description = null
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    #endregion

    #region UpdateCategoryValidator Tests

    [TestMethod]
    public async Task UpdateCategoryValidator_WithValidDTO_ReturnsOk()
    {
        // Arrange
        var validator = new UpdateCategoryValidator();
        var dto = new UpdateCategoryDTO
        {
            Id = Guid.NewGuid(),
            Name = "Updated Category",
            Description = "Updated description"
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task UpdateCategoryValidator_WithZeroId_ReturnsFailed()
    {
        // Arrange
        var validator = new UpdateCategoryValidator();
        var dto = new UpdateCategoryDTO
        {
            Id = Guid.Empty,
            Name = "Category",
            Description = "Description"
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task UpdateCategoryValidator_WithEmptyName_ReturnsFailed()
    {
        // Arrange
        var validator = new UpdateCategoryValidator();
        var dto = new UpdateCategoryDTO
        {
            Id = Guid.NewGuid(),
            Name = "",
            Description = "Description"
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    #endregion
}
