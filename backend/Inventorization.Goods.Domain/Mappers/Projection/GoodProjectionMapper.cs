using Inventorization.Base.Abstractions;
using Inventorization.Base.ADTs;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.ADTs;
using System.Linq.Expressions;

namespace Inventorization.Goods.Domain.Mappers.Projection;

/// <summary>
/// Maps Good entity to GoodProjection based on requested fields.
/// Supports selective field projection and related entity navigation with depth control.
/// </summary>
public class GoodProjectionMapper : ProjectionMapperBase<Good, GoodProjection>, IGoodProjectionMapper
{
    private readonly ICategoryProjectionMapper _categoryProjectionMapper;
    
    public GoodProjectionMapper(ICategoryProjectionMapper categoryProjectionMapper)
    {
        _categoryProjectionMapper = categoryProjectionMapper;
    }
    
    /// <summary>
    /// Returns projection expression for all fields
    /// </summary>
    protected override Expression<Func<Good, GoodProjection>> GetAllFieldsProjection(bool deep, int depth)
    {
        if (deep && depth > 0)
        {
            // Include all related entity fields with nested structure
            return g => new GoodProjection
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Sku = g.Sku,
                UnitPrice = g.UnitPrice,
                QuantityInStock = g.QuantityInStock,
                UnitOfMeasure = g.UnitOfMeasure,
                CategoryId = g.CategoryId,
                Category = g.Category != null ? new CategoryProjection
                {
                    Id = g.Category.Id,
                    Name = g.Category.Name,
                    Description = g.Category.Description,
                    ParentCategoryId = g.Category.ParentCategoryId,
                    IsActive = g.Category.IsActive,
                    CreatedAt = g.Category.CreatedAt,
                    UpdatedAt = g.Category.UpdatedAt,
                    // Recursively include ParentCategory if depth allows
                    ParentCategory = depth > 1 && g.Category.ParentCategory != null ? new CategoryProjection
                    {
                        Id = g.Category.ParentCategory.Id,
                        Name = g.Category.ParentCategory.Name,
                        Description = g.Category.ParentCategory.Description,
                        ParentCategoryId = g.Category.ParentCategory.ParentCategoryId,
                        IsActive = g.Category.ParentCategory.IsActive,
                        CreatedAt = g.Category.ParentCategory.CreatedAt,
                        UpdatedAt = g.Category.ParentCategory.UpdatedAt
                    } : null
                } : null,
                IsActive = g.IsActive,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            };
        }
        else
        {
            // Include only direct Good properties, not related entities
            return g => new GoodProjection
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Sku = g.Sku,
                UnitPrice = g.UnitPrice,
                QuantityInStock = g.QuantityInStock,
                UnitOfMeasure = g.UnitOfMeasure,
                CategoryId = g.CategoryId,
                IsActive = g.IsActive,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            };
        }
    }
    
    /// <summary>
    /// Maps all fields from Good entity to GoodProjection for in-memory mapping.
    /// </summary>
    protected override void MapAllFields(Good entity, GoodProjection result, bool deep, int maxDepth, int currentDepth)
    {
        result.Id = entity.Id;
        result.Name = entity.Name;
        result.Description = entity.Description;
        result.Sku = entity.Sku;
        result.UnitPrice = entity.UnitPrice;
        result.QuantityInStock = entity.QuantityInStock;
        result.UnitOfMeasure = entity.UnitOfMeasure;
        result.CategoryId = entity.CategoryId;
        result.IsActive = entity.IsActive;
        result.CreatedAt = entity.CreatedAt;
        result.UpdatedAt = entity.UpdatedAt;
        
        // Include related entities only if deep is true and we haven't exceeded maxDepth
        if (deep && currentDepth < maxDepth && entity.Category != null)
        {
            // Use CategoryProjectionMapper for nested Category projection
            var categoryProjection = ProjectionRequest.AllDeep(maxDepth - currentDepth - 1);
            result.Category = _categoryProjectionMapper.Map(entity.Category, categoryProjection, currentDepth + 1);
        }
    }

    protected override Expression<Func<Good, GoodProjection>> BuildSelectiveProjection(ProjectionRequest projection)
    {
        // Build a set of requested field names for O(1) lookup
        var requestedFields = new HashSet<string>(projection.Fields.Select(f => f.FieldName), StringComparer.OrdinalIgnoreCase);
        
        // Check for specific fields upfront (to avoid EF Core translation issues)
        var hasId = requestedFields.Contains("Id");
        var hasName = requestedFields.Contains("Name");
        var hasDescription = requestedFields.Contains("Description");
        var hasSku = requestedFields.Contains("Sku");
        var hasUnitPrice = requestedFields.Contains("UnitPrice");
        var hasQuantityInStock = requestedFields.Contains("QuantityInStock");
        var hasUnitOfMeasure = requestedFields.Contains("UnitOfMeasure");
        var hasCategoryId = requestedFields.Contains("CategoryId");
        var hasIsActive = requestedFields.Contains("IsActive");
        var hasCreatedAt = requestedFields.Contains("CreatedAt");
        var hasUpdatedAt = requestedFields.Contains("UpdatedAt");
        
        // Check for Category nested fields
        var hasCategoryName = requestedFields.Contains("Category.Name");
        var hasCategoryDescription = requestedFields.Contains("Category.Description");
        var hasCategoryId2 = requestedFields.Contains("Category.Id");
        var hasCategoryParentId = requestedFields.Contains("Category.ParentCategoryId");
        var hasCategoryIsActive = requestedFields.Contains("Category.IsActive");
        var hasCategoryCreatedAt = requestedFields.Contains("Category.CreatedAt");
        var hasCategoryUpdatedAt = requestedFields.Contains("Category.UpdatedAt");
        var hasAnyCategory = hasCategoryName || hasCategoryDescription || hasCategoryId2 || hasCategoryParentId || hasCategoryIsActive || hasCategoryCreatedAt || hasCategoryUpdatedAt;
        
        // Build expression with constants evaluated outside
        return g => new GoodProjection
        {
            Id = hasId ? g.Id : null,
            Name = hasName ? g.Name : null,
            Description = hasDescription ? g.Description : null,
            Sku = hasSku ? g.Sku : null,
            UnitPrice = hasUnitPrice ? g.UnitPrice : null,
            QuantityInStock = hasQuantityInStock ? g.QuantityInStock : null,
            UnitOfMeasure = hasUnitOfMeasure ? g.UnitOfMeasure : null,
            CategoryId = hasCategoryId ? g.CategoryId : null,
            Category = hasAnyCategory && g.Category != null ? new CategoryProjection
            {
                Id = hasCategoryId2 ? g.Category.Id : null,
                Name = hasCategoryName ? g.Category.Name : null,
                Description = hasCategoryDescription ? g.Category.Description : null,
                ParentCategoryId = hasCategoryParentId ? g.Category.ParentCategoryId : null,
                IsActive = hasCategoryIsActive ? g.Category.IsActive : null,
                CreatedAt = hasCategoryCreatedAt ? g.Category.CreatedAt : null,
                UpdatedAt = hasCategoryUpdatedAt ? g.Category.UpdatedAt : null
            } : null,
            IsActive = hasIsActive ? g.IsActive : null,
            CreatedAt = hasCreatedAt ? g.CreatedAt : null,
            UpdatedAt = hasUpdatedAt ? g.UpdatedAt : null
        };
    }
    
    protected override void MapField(Good entity, GoodProjection result, FieldProjection field, int currentDepth)
    {
        switch (field.FieldName)
        {
            case "Id":
                result.Id = entity.Id;
                break;
            case "Name":
                result.Name = entity.Name;
                break;
            case "Description":
                result.Description = entity.Description;
                break;
            case "Sku":
                result.Sku = entity.Sku;
                break;
            case "UnitPrice":
                result.UnitPrice = entity.UnitPrice;
                break;
            case "QuantityInStock":
                result.QuantityInStock = entity.QuantityInStock;
                break;
            case "UnitOfMeasure":
                result.UnitOfMeasure = entity.UnitOfMeasure;
                break;
            case "CategoryId":
                result.CategoryId = entity.CategoryId;
                break;
            case "Category.Id":
                if (entity.Category != null)
                {
                    result.Category ??= new CategoryProjection();
                    result.Category.Id = entity.Category.Id;
                }
                break;
            case "Category.Name":
                if (entity.Category != null)
                {
                    result.Category ??= new CategoryProjection();
                    result.Category.Name = entity.Category.Name;
                }
                break;
            case "Category.Description":
                if (entity.Category != null)
                {
                    result.Category ??= new CategoryProjection();
                    result.Category.Description = entity.Category.Description;
                }
                break;
            case "Category.ParentCategoryId":
                if (entity.Category != null)
                {
                    result.Category ??= new CategoryProjection();
                    result.Category.ParentCategoryId = entity.Category.ParentCategoryId;
                }
                break;
            case "Category.IsActive":
                if (entity.Category != null)
                {
                    result.Category ??= new CategoryProjection();
                    result.Category.IsActive = entity.Category.IsActive;
                }
                break;
            case "Category.CreatedAt":
                if (entity.Category != null)
                {
                    result.Category ??= new CategoryProjection();
                    result.Category.CreatedAt = entity.Category.CreatedAt;
                }
                break;
            case "Category.UpdatedAt":
                if (entity.Category != null)
                {
                    result.Category ??= new CategoryProjection();
                    result.Category.UpdatedAt = entity.Category.UpdatedAt;
                }
                break;
            case "IsActive":
                result.IsActive = entity.IsActive;
                break;
            case "CreatedAt":
                result.CreatedAt = entity.CreatedAt;
                break;
            case "UpdatedAt":
                result.UpdatedAt = entity.UpdatedAt;
                break;
        }
    }
}
