using Moq;
using Inventorization.Base.Abstractions;
using InventorySystem.Business.Abstractions;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.StockMovement;
using InventorySystem.Business.DataServices;
using Inventorization.Base.DTOs;
using DataAccessMovementType = InventorySystem.DataAccess.Models.MovementType;
using DTOMovementType = InventorySystem.DTOs.DTO.StockMovement.MovementType;

namespace InventorySystem.API.Tests.Services;

[TestClass]
public class StockMovementDataServiceTests
{
    private Mock<InventorySystem.DataAccess.Abstractions.IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IMapper<StockMovement, StockMovementDetailsDTO>> _mapperMock = null!;
    private Mock<IEntityCreator<StockMovement, CreateStockMovementDTO>> _creatorMock = null!;
    private Mock<ISearchQueryProvider<StockMovement, StockMovementSearchDTO>> _searchProviderMock = null!;
    private Mock<IValidator<CreateStockMovementDTO>> _createValidatorMock = null!;
    private Mock<IAuditLogger> _auditLoggerMock = null!;
    private StockMovementDataService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<InventorySystem.DataAccess.Abstractions.IUnitOfWork>();
        _mapperMock = new Mock<IMapper<StockMovement, StockMovementDetailsDTO>>();
        _creatorMock = new Mock<IEntityCreator<StockMovement, CreateStockMovementDTO>>();
        _searchProviderMock = new Mock<ISearchQueryProvider<StockMovement, StockMovementSearchDTO>>();
        _createValidatorMock = new Mock<IValidator<CreateStockMovementDTO>>();
        _auditLoggerMock = new Mock<IAuditLogger>();

        _service = new StockMovementDataService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _creatorMock.Object,
            _searchProviderMock.Object,
            _createValidatorMock.Object,
            _auditLoggerMock.Object
        );
    }

    #region GetByIdAsync Tests

    [TestMethod]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var movementId = Guid.NewGuid();
        var movement = new StockMovement
        {
            Id = movementId,
            ProductId = Guid.NewGuid(),
            Quantity = 10,
            Type = DataAccessMovementType.In
        };
        var expectedDto = new StockMovementDetailsDTO
        {
            Id = movementId,
            Quantity = 10
        };

        _unitOfWorkMock
            .Setup(u => u.StockMovements.GetByIdAsync(movementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movement);

        _mapperMock
            .Setup(m => m.Map(movement))
            .Returns(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(movementId);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(movementId, result.Data.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var movementId = Guid.NewGuid();
        _unitOfWorkMock
            .Setup(u => u.StockMovements.GetByIdAsync(movementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockMovement)null!);

        // Act
        var result = await _service.GetByIdAsync(movementId);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Stock movement not found", result.Message);
    }

    #endregion

    #region AddAsync Tests

    [TestMethod]
    public async Task AddAsync_WithValidDTO_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var createDto = new CreateStockMovementDTO
        {
            ProductId = productId,
            Quantity = 50,
            Type = DTOMovementType.In,
            Notes = "Initial stock"
        };
        var newMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = 50,
            Type = DataAccessMovementType.In,
            Notes = "Initial stock"
        };
        var expectedDto = new StockMovementDetailsDTO
        {
            Id = newMovement.Id,
            Quantity = 50
        };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Ok());

        _creatorMock
            .Setup(c => c.Create(createDto))
            .Returns(newMovement);

        _unitOfWorkMock
            .Setup(u => u.StockMovements.CreateAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newMovement);

        _mapperMock
            .Setup(m => m.Map(newMovement))
            .Returns(expectedDto);

        // Act
        var result = await _service.AddAsync(createDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Stock movement created successfully", result.Message);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task AddAsync_WithValidationError_ReturnsFailure()
    {
        // Arrange
        var createDto = new CreateStockMovementDTO
        {
            ProductId = Guid.Empty,
            Quantity = -10,
            Type = DTOMovementType.In
        };
        var validationErrors = new[] { "Product ID is required", "Quantity must be positive" };

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
    public async Task UpdateAsync_ReturnsNotSupported()
    {
        // Arrange
        var updateDto = new TestUpdateDTO { Id = Guid.NewGuid() };

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Stock movements cannot be updated", result.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [TestMethod]
    public async Task DeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var movementId = Guid.NewGuid();
        var movement = new StockMovement
        {
            Id = movementId,
            ProductId = Guid.NewGuid(),
            Quantity = 10
        };
        var deleteDto = new TestDeleteDTO { Id = movementId };

        _unitOfWorkMock
            .Setup(u => u.StockMovements.GetByIdAsync(movementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movement);

        _unitOfWorkMock
            .Setup(u => u.StockMovements.DeleteAsync(movementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(deleteDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Stock movement deleted successfully", result.Message);
    }

    [TestMethod]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFailure()
    {
        // Arrange
        var movementId = Guid.NewGuid();
        var deleteDto = new TestDeleteDTO { Id = movementId };

        _unitOfWorkMock
            .Setup(u => u.StockMovements.GetByIdAsync(movementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockMovement)null!);

        // Act
        var result = await _service.DeleteAsync(deleteDto);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Stock movement not found", result.Message);
    }

    #endregion

    #region SearchAsync Tests

    [TestMethod]
    public async Task SearchAsync_ReturnsPagedResults()
    {
        // Arrange
        var searchDto = new StockMovementSearchDTO
        {
            Page = new PageDTO { PageNumber = 1, PageSize = 10 }
        };
        var movements = new List<StockMovement>
        {
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 10,
                Type = DataAccessMovementType.In
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 5,
                Type = DataAccessMovementType.Out
            }
        };
        var dtos = movements.Select(m => new StockMovementDetailsDTO
        {
            Id = m.Id,
            Quantity = m.Quantity
        }).ToList();

        _unitOfWorkMock
            .Setup(u => u.StockMovements.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(movements);

        _searchProviderMock
            .Setup(s => s.GetSearchExpression(searchDto))
            .Returns(m => true);

        _mapperMock
            .Setup(m => m.Map(It.IsAny<StockMovement>()))
            .Returns((StockMovement m) => dtos.FirstOrDefault(d => d.Id == m.Id) ?? dtos[0]);

        // Act
        var result = await _service.SearchAsync(searchDto);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Items.Count);
    }

    #endregion
}

public class TestUpdateDTO : UpdateDTO
{
}

