using InventorySystem.Business.Mappers;
using InventorySystem.Business.Creators;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.StockMovement;
using InventorySystem.DataAccess.Abstractions;
using Moq;
using DataAccessMovementType = InventorySystem.DataAccess.Models.MovementType;
using DTOMovementType = InventorySystem.DTOs.DTO.StockMovement.MovementType;

namespace InventorySystem.API.Tests.Mappers;

[TestClass]
public class StockMovementMapperTests
{
    private StockMovementMapper _mapper = null!;

    [TestInitialize]
    public void Setup()
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        _mapper = new StockMovementMapper(mockUnitOfWork.Object);
    }

    [TestMethod]
    public void GetProjection_WithValidMovement_ReturnsCorrectDTO()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Test Product" };
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = 100,
            Type = DataAccessMovementType.In,
            Notes = "Stock received",
            CreatedAt = DateTime.UtcNow,
            Product = product
        };
        
        var projection = _mapper.GetProjection();
        var movements = new List<StockMovement> { movement }.AsQueryable();

        // Act
        var result = movements.Select(projection).FirstOrDefault();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(movement.Id, result.Id);
        Assert.AreEqual(movement.Quantity, result.Quantity);
        Assert.AreEqual(movement.ProductId, result.ProductId);
        Assert.AreEqual("Test Product", result.ProductName);
    }

    [TestMethod]
    public void GetProjection_WithNullProduct_ReturnsEmptyProductName()
    {
        // Arrange
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 50,
            Type = DataAccessMovementType.Out,
            Notes = null,
            Product = null
        };
        
        var projection = _mapper.GetProjection();
        var movements = new List<StockMovement> { movement }.AsQueryable();

        // Act
        var result = movements.Select(projection).FirstOrDefault();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.Notes);
        Assert.AreEqual(string.Empty, result.ProductName);
    }

    [TestMethod]
    public void GetProjection_WithOutboundMovement_MapsCorrectly()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Sold Product" };
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = 25,
            Type = DataAccessMovementType.Out,
            Product = product
        };
        
        var projection = _mapper.GetProjection();
        var movements = new List<StockMovement> { movement }.AsQueryable();

        // Act
        var result = movements.Select(projection).FirstOrDefault();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(25, result.Quantity);
        Assert.AreEqual(movement.ProductId, result.ProductId);
        Assert.AreEqual("Sold Product", result.ProductName);
    }

    [TestMethod]
    public void GetProjection_ReturnsValidExpression()
    {
        // Arrange
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 100,
            Type = DataAccessMovementType.In
        };
        var projection = _mapper.GetProjection();
        var movements = new List<StockMovement> { movement }.AsQueryable();

        // Act
        var result = movements.Select(projection).FirstOrDefault();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(movement.Id, result.Id);
        Assert.AreEqual(movement.Quantity, result.Quantity);
    }
}

[TestClass]
public class StockMovementCreatorTests
{
    private StockMovementCreator _creator = null!;

    [TestInitialize]
    public void Setup()
    {
        _creator = new StockMovementCreator();
    }

    [TestMethod]
    public void Create_WithValidDTO_CreatesMovementCorrectly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var dto = new CreateStockMovementDTO
        {
            ProductId = productId,
            Quantity = 100,
            Type = DTOMovementType.In,
            Notes = "Initial stock"
        };

        // Act
        var movement = _creator.Create(dto);

        // Assert
        Assert.IsNotNull(movement);
        Assert.AreNotEqual(Guid.Empty, movement.Id);
        Assert.AreEqual(dto.ProductId, movement.ProductId);
        Assert.AreEqual(dto.Quantity, movement.Quantity);
        Assert.AreEqual((DataAccessMovementType)dto.Type, movement.Type);
        Assert.AreEqual(dto.Notes, movement.Notes);
    }

    [TestMethod]
    public void Create_GeneratesUniqueId()
    {
        // Arrange
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 50,
            Type = DTOMovementType.Out
        };

        // Act
        var movement1 = _creator.Create(dto);
        var movement2 = _creator.Create(dto);

        // Assert
        Assert.AreNotEqual(movement1.Id, movement2.Id);
    }

    [TestMethod]
    public void Create_WithoutNotes_CreatesMovement()
    {
        // Arrange
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 25,
            Type = DTOMovementType.In,
            Notes = null
        };

        // Act
        var movement = _creator.Create(dto);

        // Assert
        Assert.IsNotNull(movement);
        Assert.IsNull(movement.Notes);
    }

    [TestMethod]
    public void Create_WithInboundMovement_SetsCorrectType()
    {
        // Arrange
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1000,
            Type = DTOMovementType.In,
            Notes = "Purchase order"
        };

        // Act
        var movement = _creator.Create(dto);

        // Assert
        Assert.AreEqual(DataAccessMovementType.In, movement.Type);
    }

    [TestMethod]
    public void Create_WithOutboundMovement_SetsCorrectType()
    {
        // Arrange
        var dto = new CreateStockMovementDTO
        {
            ProductId = Guid.NewGuid(),
            Quantity = 50,
            Type = DTOMovementType.Out,
            Notes = "Sales order"
        };

        // Act
        var movement = _creator.Create(dto);

        // Assert
        Assert.AreEqual(DataAccessMovementType.Out, movement.Type);
    }
}
