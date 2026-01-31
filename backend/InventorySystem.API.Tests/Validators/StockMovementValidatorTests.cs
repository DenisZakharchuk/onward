using InventorySystem.Business.Validators;
using InventorySystem.DTOs.DTO.StockMovement;
using DTOMovementType = InventorySystem.DTOs.DTO.StockMovement.MovementType;

namespace InventorySystem.API.Tests.Validators;

[TestClass]
public class StockMovementValidatorTests
{
    #region CreateStockMovementValidator Tests

    [TestMethod]
    public async Task CreateStockMovementValidator_WithValidDTO_ReturnsOk()
    {
        // Arrange
        var validator = new CreateStockMovementValidator();
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 100,
            Type = DTOMovementType.In,
            Notes = "Stock received"
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task CreateStockMovementValidator_WithEmptyProductId_ReturnsFailed()
    {
        // Arrange
        var validator = new CreateStockMovementValidator();
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.Empty,
            Quantity = 100,
            Type = DTOMovementType.In
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task CreateStockMovementValidator_WithZeroQuantity_ReturnsFailed()
    {
        // Arrange
        var validator = new CreateStockMovementValidator();
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 0,
            Type = DTOMovementType.In
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task CreateStockMovementValidator_WithNegativeQuantity_ReturnsFailed()
    {
        // Arrange
        var validator = new CreateStockMovementValidator();
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = -50,
            Type = DTOMovementType.Out
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task CreateStockMovementValidator_WithValidInboundMovement_ReturnsOk()
    {
        // Arrange
        var validator = new CreateStockMovementValidator();
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1000,
            Type = DTOMovementType.In,
            Notes = "Purchase order #12345"
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task CreateStockMovementValidator_WithValidOutboundMovement_ReturnsOk()
    {
        // Arrange
        var validator = new CreateStockMovementValidator();
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 50,
            Type = DTOMovementType.Out,
            Notes = "Sales order #67890"
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task CreateStockMovementValidator_WithNullNotes_ReturnsOk()
    {
        // Arrange
        var validator = new CreateStockMovementValidator();
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 100,
            Type = DTOMovementType.In,
            Notes = null
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task CreateStockMovementValidator_WithLargeQuantity_ReturnsOk()
    {
        // Arrange
        var validator = new CreateStockMovementValidator();
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 999999,
            Type = DTOMovementType.In
        };

        // Act
        var result = await validator.ValidateAsync(dto, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    #endregion
}
