using InventorySystem.Business.SearchProviders;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.StockMovement;
using DataAccessMovementType = InventorySystem.DataAccess.Models.MovementType;
using DTOMovementType = InventorySystem.DTOs.DTO.StockMovement.MovementType;

namespace InventorySystem.API.Tests.SearchProviders;

[TestClass]
public class StockMovementSearchProviderTests
{
    private StockMovementSearchProvider _provider = null!;

    [TestInitialize]
    public void Setup()
    {
        _provider = new StockMovementSearchProvider();
    }

    [TestMethod]
    public void GetSearchExpression_WithNoFilter_ReturnsAllMovements()
    {
        // Arrange
        var searchDto = new StockMovementSearchDTO();
        var movements = new List<StockMovement>
        {
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 100,
                Type = DataAccessMovementType.In
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 50,
                Type = DataAccessMovementType.Out
            }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = movements.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetSearchExpression_WithProductIdFilter_ReturnsMatchingMovements()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var searchDto = new StockMovementSearchDTO { ProductId = productId1 };
        var movements = new List<StockMovement>
        {
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = productId1,
                Quantity = 100,
                Type = DataAccessMovementType.In
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = productId2,
                Quantity = 50,
                Type = DataAccessMovementType.Out
            }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = movements.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(productId1, result[0].ProductId);
    }

    [TestMethod]
    public void GetSearchExpression_WithMovementTypeFilter_ReturnsMatchingMovements()
    {
        // Arrange
        var searchDto = new StockMovementSearchDTO { Type = DTOMovementType.In };
        var movements = new List<StockMovement>
        {
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 100,
                Type = DataAccessMovementType.In
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 50,
                Type = DataAccessMovementType.Out
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 25,
                Type = DataAccessMovementType.In
            }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = movements.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(m => m.Type == DataAccessMovementType.In));
    }

    [TestMethod]
    public void GetSearchExpression_WithOutboundFilter_ReturnsOutboundMovements()
    {
        // Arrange
        var searchDto = new StockMovementSearchDTO { Type = DTOMovementType.Out };
        var movements = new List<StockMovement>
        {
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 100,
                Type = DataAccessMovementType.In
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 50,
                Type = DataAccessMovementType.Out
            }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = movements.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(DataAccessMovementType.Out, result[0].Type);
    }

    [TestMethod]
    public void GetSearchExpression_WithMultipleFilters_ReturnsFiltered()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var searchDto = new StockMovementSearchDTO
        {
            ProductId = productId,
            Type = DTOMovementType.In
        };
        var movements = new List<StockMovement>
        {
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = 100,
                Type = DataAccessMovementType.In
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = 50,
                Type = DataAccessMovementType.Out
            },
            new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 75,
                Type = DataAccessMovementType.In
            }
        };

        // Act
        var expression = _provider.GetSearchExpression(searchDto);
        var compiled = expression.Compile();
        var result = movements.Where(compiled).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(productId, result[0].ProductId);
        Assert.AreEqual(DataAccessMovementType.In, result[0].Type);
    }
}
