using InventorySystem.Business.Abstractions;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs;

namespace InventorySystem.Business.Services;

public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger? _auditLogger;

    public ProductService(IUnitOfWork unitOfWork, IAuditLogger? auditLogger = null)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        
        return products.Select(p => MapToDto(p, categories.FirstOrDefault(c => c.Id == p.CategoryId)));
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null) return null;

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId, cancellationToken);
        return MapToDto(product, category);
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetLowStockProductsAsync(cancellationToken);
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        
        return products.Select(p => MapToDto(p, categories.FirstOrDefault(c => c.Id == p.CategoryId)));
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            CurrentStock = dto.InitialStock,
            MinimumStock = dto.MinimumStock
        };

        var created = await _unitOfWork.Products.CreateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log (fire-and-forget)
        _ = LogAuditAsync("ProductCreated", "Product", created.Id.ToString(), new Dictionary<string, object>
        {
            { "name", created.Name },
            { "price", created.Price },
            { "initialStock", created.CurrentStock }
        });

        var category = await _unitOfWork.Categories.GetByIdAsync(created.CategoryId, cancellationToken);
        return MapToDto(created, category);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        var changes = new Dictionary<string, object>();
        if (existing.Name != dto.Name) changes["name"] = new { old = existing.Name, @new = dto.Name };
        if (existing.Price != dto.Price) changes["price"] = new { old = existing.Price, @new = dto.Price };

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.SKU = dto.SKU;
        existing.Price = dto.Price;
        existing.CategoryId = dto.CategoryId;
        existing.MinimumStock = dto.MinimumStock;

        var updated = await _unitOfWork.Products.UpdateAsync(existing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        _ = LogAuditAsync("ProductUpdated", "Product", id.ToString(), changes);

        var category = await _unitOfWork.Categories.GetByIdAsync(updated.CategoryId, cancellationToken);
        return MapToDto(updated, category);
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        
        var result = await _unitOfWork.Products.DeleteAsync(id, cancellationToken);
        if (result)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Audit log
            _ = LogAuditAsync("ProductDeleted", "Product", id.ToString(), new Dictionary<string, object>
            {
                { "name", product?.Name ?? "Unknown" }
            });
        }
        return result;
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

    private static ProductDto MapToDto(Product product, Category? category)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? "Unknown",
            CurrentStock = product.CurrentStock,
            MinimumStock = product.MinimumStock,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
