using Moq;
using Inventorization.Base.Abstractions;
using InventorySystem.Business.Abstractions;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Category;
using InventorySystem.Business.DataServices;
using Inventorization.Base.DTOs;

namespace InventorySystem.API.Tests.Services;

[TestClass]
public class CategoryDataServiceTests
{
    private Mock<InventorySystem.DataAccess.Abstractions.IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IMapper<Category, CategoryDetailsDTO>> _mapperMock = null!;
    private Mock<IEntityCreator<Category, CreateCategoryDTO>> _creatorMock = null!;
    private Mock<IEntityModifier<Category, UpdateCategoryDTO>> _modifierMock = null!;
    private Mock<ISearchQueryProvider<Category, CategorySearchDTO>> _searchProviderMock = null!;
    private Mock<IValidator<CreateCategoryDTO>> _createValidatorMock = null!;
    private Mock<IValidator<UpdateCategoryDTO>> _updateValidatorMock = null!;
    private Mock<IAuditLogger> _auditLoggerMock = null!;
    private CategoryDataService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<InventorySystem.DataAccess.Abstractions.IUnitOfWork>();
        _mapperMock = new Mock<IMapper<Category, CategoryDetailsDTO>>();
        _creatorMock = new Mock<IEntityCreator<Category, CreateCategoryDTO>>();
        _modifierMock = new Mock<IEntityModifier<Category, UpdateCategoryDTO>>();
        _searchProviderMock = new Mock<ISearchQueryProvider<Category, CategorySearchDTO>>();
        _createValidatorMock = new Mock<IValidator<CreateCategoryDTO>>();
        _updateValidatorMock = new Mock<IValidator<UpdateCategoryDTO>>();
        _auditLoggerMock = new Mock<IAuditLogger>();

        _service = new CategoryDataService(
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
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics" };
        var expectedDto = new CategoryDetailsDTO { Id = categoryId, Name = "Electronics" };

        _unitOfWorkMock
            .Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mapperMock
            .Setup(m => m.Map(category))
            .Returns(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(categoryId);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(categoryId, result.Data.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _unitOfWorkMock
            .Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        // Act
        var result = await _service.GetByIdAsync(categoryId);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Category not found", result.Message);
    }

    #endregion

    #region AddAsync Tests

    [TestMethod]
    public async Task AddAsync_WithValidDTO_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateCategoryDTO
        {
            Name = "New Category",
            Description = "New Description"
        };
        var newCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "New Category",
            Description = "New Description"
        };
        var expectedDto = new CategoryDetailsDTO
        {
            Id = newCategory.Id,
            Name = "New Category"
        };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Ok());

        _creatorMock
            .Setup(c => c.Create(createDto))
            .Returns(newCategory);

        _unitOfWorkMock
            .Setup(u => u.Categories.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        _mapperMock
            .Setup(m => m.Map(newCategory))
            .Returns(expectedDto);

        // Act
        var result = await _service.AddAsync(createDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task AddAsync_WithValidationError_ReturnsFailure()
    {
        // Arrange
        var createDto = new CreateCategoryDTO { Name = "" };
        var validationErrors = new[] { "Category name is required" };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.WithErrors(validationErrors));

        // Act
        var result = await _service.AddAsync(createDto);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Validation failed", result.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [TestMethod]
    public async Task UpdateAsync_WithValidDTO_ReturnsSuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDTO
        {
            Id = categoryId,
            Name = "Updated Category"
        };
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Old Category"
        };
        var updatedDto = new CategoryDetailsDTO
        {
            Id = categoryId,
            Name = "Updated Category"
        };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Ok());

        _unitOfWorkMock
            .Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _modifierMock
            .Setup(m => m.Modify(existingCategory, updateDto));

        _unitOfWorkMock
            .Setup(u => u.Categories.UpdateAsync(existingCategory, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _mapperMock
            .Setup(m => m.Map(existingCategory))
            .Returns(updatedDto);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Category updated successfully", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_WithNonExistentCategory_ReturnsFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDTO { Id = categoryId, Name = "Category" };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Ok());

        _unitOfWorkMock
            .Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Category not found", result.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [TestMethod]
    public async Task DeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Test Category" };
        var deleteDto = new TestDeleteDTO { Id = categoryId };

        _unitOfWorkMock
            .Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _unitOfWorkMock
            .Setup(u => u.Categories.DeleteAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(deleteDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Category deleted successfully", result.Message);
    }

    [TestMethod]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var deleteDto = new TestDeleteDTO { Id = categoryId };

        _unitOfWorkMock
            .Setup(u => u.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        // Act
        var result = await _service.DeleteAsync(deleteDto);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Category not found", result.Message);
    }

    #endregion

    #region SearchAsync Tests

    [TestMethod]
    public async Task SearchAsync_ReturnsPagedResults()
    {
        // Arrange
        var searchDto = new CategorySearchDTO
        {
            Page = new PageDTO { PageNumber = 1, PageSize = 10 }
        };
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Electronics" },
            new Category { Id = Guid.NewGuid(), Name = "Books" }
        };
        var dtos = categories.Select(c => new CategoryDetailsDTO
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

        _unitOfWorkMock
            .Setup(u => u.Categories.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _searchProviderMock
            .Setup(s => s.GetSearchExpression(searchDto))
            .Returns(c => true);

        _mapperMock
            .Setup(m => m.Map(It.IsAny<Category>()))
            .Returns((Category c) => dtos.FirstOrDefault(d => d.Id == c.Id) ?? dtos[0]);

        // Act
        var result = await _service.SearchAsync(searchDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Items.Count);
    }

    #endregion
}
