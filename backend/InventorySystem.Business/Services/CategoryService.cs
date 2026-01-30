using InventorySystem.Business.Abstractions;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs;

namespace InventorySystem.Business.Services;

public class CategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger? _auditLogger;

    public CategoryService(IUnitOfWork unitOfWork, IAuditLogger? auditLogger = null)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        return category != null ? MapToDto(category) : null;
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        var created = await _unitOfWork.Categories.CreateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        _ = LogAuditAsync("CategoryCreated", "Category", created.Id.ToString(), new Dictionary<string, object>
        {
            { "name", created.Name }
        });

        return MapToDto(created);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        var changes = new Dictionary<string, object>();
        if (existing.Name != dto.Name) changes["name"] = new { old = existing.Name, @new = dto.Name };

        existing.Name = dto.Name;
        existing.Description = dto.Description;

        var updated = await _unitOfWork.Categories.UpdateAsync(existing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        _ = LogAuditAsync("CategoryUpdated", "Category", id.ToString(), changes);

        return MapToDto(updated);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        
        var result = await _unitOfWork.Categories.DeleteAsync(id, cancellationToken);
        if (result)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Audit log
            _ = LogAuditAsync("CategoryDeleted", "Category", id.ToString(), new Dictionary<string, object>
            {
                { "name", category?.Name ?? "Unknown" }
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

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
