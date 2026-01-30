using InventorySystem.Business.Abstractions;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs;

namespace InventorySystem.Business.Services;

public class StockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger? _auditLogger;

    public StockService(IUnitOfWork unitOfWork, IAuditLogger? auditLogger = null)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<StockMovementDto> CreateStockMovementAsync(CreateStockMovementDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {dto.ProductId} not found");
        }

        // Calculate new stock level
        var newStock = dto.Type switch
        {
            DTOs.MovementType.In => product.CurrentStock + dto.Quantity,
            DTOs.MovementType.Out => product.CurrentStock - dto.Quantity,
            DTOs.MovementType.Adjustment => dto.Quantity,
            _ => product.CurrentStock
        };

        if (newStock < 0)
        {
            throw new InvalidOperationException("Stock cannot be negative");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create movement record
            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                Type = (DataAccess.Models.MovementType)dto.Type,
                Quantity = dto.Quantity,
                Notes = dto.Notes
            };

            var created = await _unitOfWork.StockMovements.CreateAsync(movement, cancellationToken);

            // Update product stock
            await _unitOfWork.Products.UpdateStockAsync(dto.ProductId, newStock, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Audit log
            _ = LogAuditAsync("StockMovementCreated", "StockMovement", created.Id.ToString(), new Dictionary<string, object>
            {
                { "productId", product.Id },
                { "productName", product.Name },
                { "type", dto.Type },
                { "quantity", dto.Quantity },
                { "previousStock", product.CurrentStock },
                { "newStock", newStock }
            });

            return MapToDto(created, product);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<StockMovementDto>> GetProductMovementsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var movements = await _unitOfWork.StockMovements.GetByProductIdAsync(productId, cancellationToken);
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);

        return movements.Select(m => MapToDto(m, product));
    }

    public async Task<IEnumerable<StockMovementDto>> GetAllMovementsAsync(CancellationToken cancellationToken = default)
    {
        var movements = await _unitOfWork.StockMovements.GetAllAsync(cancellationToken);
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);

        return movements.Select(m =>
        {
            var product = products.FirstOrDefault(p => p.Id == m.ProductId);
            return MapToDto(m, product);
        });
    }

    private Task LogAuditAsync(string action, string entityType, string entityId, Dictionary<string, object> changes)
    {
        if (_auditLogger == null) return Task.CompletedTask;

        var entry = new AuditLogEntry
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Changes = changes,
            UserId = "system"
        };

        return _auditLogger.LogAsync(entry);
    }

    private static StockMovementDto MapToDto(StockMovement movement, Product? product)
    {
        return new StockMovementDto
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            ProductName = product?.Name ?? "Unknown",
            Type = (DTOs.MovementType)movement.Type,
            Quantity = movement.Quantity,
            Notes = movement.Notes,
            CreatedAt = movement.CreatedAt
        };
    }
}
