using Moq;
using Inventorization.Base.Abstractions;
using InventorySystem.Business.Abstractions;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;
using InventorySystem.Business.DataServices;
using Inventorization.Base.DTOs;

namespace InventorySystem.API.Tests.Services;

[TestClass]
public class ProductDataServiceTests
{
    private Mock<InventorySystem.DataAccess.Abstractions.IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IMapper<Product, ProductDetailsDTO>> _mapperMock = null!;
    private Mock<IEntityCreator<Product, CreateProductDTO>> _creatorMock = null!;
    private Mock<IEntityModifier<Product, UpdateProductDTO>> _modifierMock = null!;
    private Mock<ISearchQueryProvider<Product, ProductSearchDTO>> _searchProviderMock = null!;
    private Mock<IValidator<CreateProductDTO>> _createValidatorMock = null!;
    private Mock<IValidator<UpdateProductDTO>> _updateValidatorMock = null!;
    private Mock<IAuditLogger> _auditLoggerMock = null!;
    private ProductDataService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<InventorySystem.DataAccess.Abstractions.IUnitOfWork>();
        _mapperMock = new Mock<IMapper<Product, ProductDetailsDTO>>();
        _creatorMock = new Mock<IEntityCreator<Product, CreateProductDTO>>();
        _modifierMock = new Mock<IEntityModifier<Product, UpdateProductDTO>>();
        _searchProviderMock = new Mock<ISearchQueryProvider<Product, ProductSearchDTO>>();
        _createValidatorMock = new Mock<IValidator<CreateProductDTO>>();
        _updateValidatorMock = new Mock<IValidator<UpdateProductDTO>>();
        _auditLoggerMock = new Mock<IAuditLogger>();

        _service = new ProductDataService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _creatorMock.Object,
            _modifierMock.Object,
            _searchProviderMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _auditLoggerMock.Object
        );
    }

    #region GetByIdAsync Tests

    [TestMethod]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test Product", Price = 100m };
        var expectedDto = new ProductDetailsDTO { Id = productId, Name = "Test Product", Price = 100m };

        _unitOfWorkMock
            .Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(m => m.Map(product))
            .Returns(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(productId);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(productId, result.Data.Id);
        Assert.AreEqual("Test Product", result.Data.Name);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _unitOfWorkMock
            .Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        var result = await _service.GetByIdAsync(productId);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Product not found", result.Message);
        Assert.IsNull(result.Data);
    }

    #endregion

    #region AddAsync Tests

    [TestMethod]
    public async Task AddAsync_WithValidDTO_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateProductDTO 
        { 
            Name = "New Product", 
            Price = 50m, 
            CategoryId = Guid.NewGuid(),
            InitialStock = 10,
            MinimumStock = 2
        };
        var newProduct = new Product 
        { 
            Id = Guid.NewGuid(), 
            Name = "New Product", 
            Price = 50m,
            CurrentStock = 10,
            MinimumStock = 2
        };
        var expectedDto = new ProductDetailsDTO 
        { 
            Id = newProduct.Id, 
            Name = "New Product", 
            Price = 50m 
        };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Ok());

        _creatorMock
            .Setup(c => c.Create(createDto))
            .Returns(newProduct);

        _unitOfWorkMock
            .Setup(u => u.Products.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newProduct);

        _mapperMock
            .Setup(m => m.Map(newProduct))
            .Returns(expectedDto);

        // Act
        var result = await _service.AddAsync(createDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Product created successfully", result.Message);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task AddAsync_WithInvalidDTO_ReturnsFailure()
    {
        // Arrange
        var createDto = new CreateProductDTO { Name = "", Price = -10m };
        var validationErrors = new[] { "Name is required", "Price must be positive" };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.WithErrors(validationErrors));

        // Act
        var result = await _service.AddAsync(createDto);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Validation failed", result.Message);
        Assert.IsTrue(result.Errors.Count > 0);
        _unitOfWorkMock.Verify(u => u.Products.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateAsync Tests

    [TestMethod]
    public async Task UpdateAsync_WithValidDTO_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateDto = new UpdateProductDTO
        {
            Id = productId,
            Name = "Updated Product",
            Price = 75m,
            CategoryId = Guid.NewGuid(),
            MinimumStock = 5
        };
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Old Product",
            Price = 50m,
            MinimumStock = 2
        };
        var updatedDto = new ProductDetailsDTO
        {
            Id = productId,
            Name = "Updated Product",
            Price = 75m
        };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Ok());

        _unitOfWorkMock
            .Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _modifierMock
            .Setup(m => m.Modify(existingProduct, updateDto))
            .Callback<Product, UpdateProductDTO>((p, dto) => p.Name = dto.Name);

        _unitOfWorkMock
            .Setup(u => u.Products.UpdateAsync(existingProduct, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _mapperMock
            .Setup(m => m.Map(existingProduct))
            .Returns(updatedDto);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Product updated successfully", result.Message);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WithNonExistentProduct_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateDto = new UpdateProductDTO { Id = productId, Name = "Product" };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Ok());

        _unitOfWorkMock
            .Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Product not found", result.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [TestMethod]
    public async Task DeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test Product" };
        var deleteDto = new TestDeleteDTO { Id = productId };

        _unitOfWorkMock
            .Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(u => u.Products.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(deleteDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data);
        Assert.AreEqual("Product deleted successfully", result.Message);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var deleteDto = new TestDeleteDTO { Id = productId };

        _unitOfWorkMock
            .Setup(u => u.Products.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        var result = await _service.DeleteAsync(deleteDto);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Product not found", result.Message);
    }

    #endregion

    #region SearchAsync Tests

    [TestMethod]
    public async Task SearchAsync_ReturnsPagedResults()
    {
        // Arrange
        var searchDto = new ProductSearchDTO 
        { 
            NameFilter = "Test",
            Page = new PageDTO { PageNumber = 1, PageSize = 10 }
        };
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Test Product 1", Price = 50m },
            new Product { Id = Guid.NewGuid(), Name = "Test Product 2", Price = 75m }
        };
        var dtos = products.Select(p => new ProductDetailsDTO 
        { 
            Id = p.Id, 
            Name = p.Name, 
            Price = p.Price 
        }).ToList();

        _unitOfWorkMock
            .Setup(u => u.Products.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _searchProviderMock
            .Setup(s => s.GetSearchExpression(searchDto))
            .Returns(p => p.Name.Contains("Test"));

        _mapperMock
            .Setup(m => m.Map(It.IsAny<Product>()))
            .Returns((Product p) => dtos.First(d => d.Id == p.Id));

        // Act
        var result = await _service.SearchAsync(searchDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Items.Count);
        Assert.AreEqual(2, result.Data.TotalCount);
    }

    #endregion
}

/// <summary>
/// Concrete implementation of DeleteDTO for testing
/// </summary>
public class TestDeleteDTO : DeleteDTO
{
}
