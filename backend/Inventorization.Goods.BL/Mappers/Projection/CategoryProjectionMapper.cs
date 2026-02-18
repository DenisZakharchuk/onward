using Inventorization.Base.Abstractions;
using Inventorization.Base.ADTs;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.ADTs;
using System.Linq.Expressions;

namespace Inventorization.Goods.BL.Mappers.Projection;

/// <summary>
/// Maps Category entity to CategoryProjection DTO based on projection specifications.
/// Supports both EF Core query translation (IQueryable) and in-memory projection.
/// </summary>
public class CategoryProjectionMapper : ProjectionMapperBase<Category, CategoryProjection>, ICategoryProjectionMapper
{
    public CategoryProjectionMapper()
    {
    }

    /// <summary>
    /// Maps all fields from Category entity to CategoryProjection for in-memory mapping.
    /// </summary>
    protected override void MapAllFields(Category entity, CategoryProjection result, bool deep, int maxDepth, int currentDepth)
    {
        // Map all fields
        result.Id = entity.Id;
        result.Name = entity.Name;
        result.Description = entity.Description;
        result.ParentCategoryId = entity.ParentCategoryId;
        result.IsActive = entity.IsActive;
        result.CreatedAt = entity.CreatedAt;
        result.UpdatedAt = entity.UpdatedAt;
        
        // Include related entities only if deep is true and we haven't exceeded maxDepth
        if (deep && currentDepth < maxDepth && entity.ParentCategory != null)
        {
            // Recursively map ParentCategory with reduced depth
            var parentProjection = ProjectionRequest.AllDeep(maxDepth - currentDepth - 1);
            result.ParentCategory = Map(entity.ParentCategory, parentProjection, currentDepth + 1);
        }
    }
    
    /// <summary>
    /// Returns projection expression for all fields
    /// </summary>
    protected override Expression<Func<Category, CategoryProjection>> GetAllFieldsProjection(bool deep, int depth)
    {
        if (deep && depth > 0)
        {
            // Include all related entity fields with nested structure
            return c => new CategoryProjection
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                // Recursively include ParentCategory if depth allows
                ParentCategory = depth > 1 && c.ParentCategory != null ? new CategoryProjection
                {
                    Id = c.ParentCategory.Id,
                    Name = c.ParentCategory.Name,
                    Description = c.ParentCategory.Description,
                    ParentCategoryId = c.ParentCategory.ParentCategoryId,
                    IsActive = c.ParentCategory.IsActive,
                    CreatedAt = c.ParentCategory.CreatedAt,
                    UpdatedAt = c.ParentCategory.UpdatedAt,
                    // Continue recursion for depth > 2
                    ParentCategory = depth > 2 && c.ParentCategory.ParentCategory != null ? new CategoryProjection
                    {
                        Id = c.ParentCategory.ParentCategory.Id,
                        Name = c.ParentCategory.ParentCategory.Name,
                        Description = c.ParentCategory.ParentCategory.Description,
                        ParentCategoryId = c.ParentCategory.ParentCategory.ParentCategoryId,
                        IsActive = c.ParentCategory.ParentCategory.IsActive,
                        CreatedAt = c.ParentCategory.ParentCategory.CreatedAt,
                        UpdatedAt = c.ParentCategory.ParentCategory.UpdatedAt
                    } : null
                } : null
            };
        }
        else
        {
            // Include only direct Category properties, not related entities
            return c => new CategoryProjection
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }
    }
    
    /// <summary>
    /// Builds projection expression for selective fields
    /// </summary>
    protected override Expression<Func<Category, CategoryProjection>> BuildSelectiveProjection(ProjectionRequest projection)
    {
        // Build a set of requested field names for O(1) lookup
        var requestedFields = new HashSet<string>(projection.Fields.Select(f => f.FieldName), StringComparer.OrdinalIgnoreCase);
        
        // Check for specific fields upfront (to avoid EF Core translation issues)
        var hasId = requestedFields.Contains("Id");
        var hasName = requestedFields.Contains("Name");
        var hasDescription = requestedFields.Contains("Description");
        var hasParentCategoryId = requestedFields.Contains("ParentCategoryId");
        var hasIsActive = requestedFields.Contains("IsActive");
        var hasCreatedAt = requestedFields.Contains("CreatedAt");
        var hasUpdatedAt = requestedFields.Contains("UpdatedAt");
        
        // Check for ParentCategory nested fields
        var hasParentCategoryName = requestedFields.Contains("ParentCategory.Name");
        var hasParentCategoryDescription = requestedFields.Contains("ParentCategory.Description");
        var hasParentCategoryId2 = requestedFields.Contains("ParentCategory.Id");
        var hasParentCategoryParentId = requestedFields.Contains("ParentCategory.ParentCategoryId");
        var hasParentCategoryIsActive = requestedFields.Contains("ParentCategory.IsActive");
        var hasParentCategoryCreatedAt = requestedFields.Contains("ParentCategory.CreatedAt");
        var hasParentCategoryUpdatedAt = requestedFields.Contains("ParentCategory.UpdatedAt");
        var hasAnyParentCategory = hasParentCategoryName || hasParentCategoryDescription || hasParentCategoryId2 
            || hasParentCategoryParentId || hasParentCategoryIsActive || hasParentCategoryCreatedAt || hasParentCategoryUpdatedAt;
        
        // Build expression with constants evaluated outside
        return c => new CategoryProjection
        {
            Id = hasId ? c.Id : null,
            Name = hasName ? c.Name : null,
            Description = hasDescription ? c.Description : null,
            ParentCategoryId = hasParentCategoryId ? c.ParentCategoryId : null,
            IsActive = hasIsActive ? c.IsActive : null,
            CreatedAt = hasCreatedAt ? c.CreatedAt : null,
            UpdatedAt = hasUpdatedAt ? c.UpdatedAt : null,
            ParentCategory = hasAnyParentCategory && c.ParentCategory != null ? new CategoryProjection
            {
                Id = hasParentCategoryId2 ? c.ParentCategory.Id : null,
                Name = hasParentCategoryName ? c.ParentCategory.Name : null,
                Description = hasParentCategoryDescription ? c.ParentCategory.Description : null,
                ParentCategoryId = hasParentCategoryParentId ? c.ParentCategory.ParentCategoryId : null,
                IsActive = hasParentCategoryIsActive ? c.ParentCategory.IsActive : null,
                CreatedAt = hasParentCategoryCreatedAt ? c.ParentCategory.CreatedAt : null,
                UpdatedAt = hasParentCategoryUpdatedAt ? c.ParentCategory.UpdatedAt : null
            } : null
        };
    }
    
    protected override void MapField(Category entity, CategoryProjection result, FieldProjection field, int currentDepth)
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
            case "ParentCategoryId":
                result.ParentCategoryId = entity.ParentCategoryId;
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
            case "ParentCategory.Id":
                if (entity.ParentCategory != null)
                {
                    result.ParentCategory ??= new CategoryProjection();
                    result.ParentCategory.Id = entity.ParentCategory.Id;
                }
                break;
            case "ParentCategory.Name":
                if (entity.ParentCategory != null)
                {
                    result.ParentCategory ??= new CategoryProjection();
                    result.ParentCategory.Name = entity.ParentCategory.Name;
                }
                break;
            case "ParentCategory.Description":
                if (entity.ParentCategory != null)
                {
                    result.ParentCategory ??= new CategoryProjection();
                    result.ParentCategory.Description = entity.ParentCategory.Description;
                }
                break;
            case "ParentCategory.ParentCategoryId":
                if (entity.ParentCategory != null)
                {
                    result.ParentCategory ??= new CategoryProjection();
                    result.ParentCategory.ParentCategoryId = entity.ParentCategory.ParentCategoryId;
                }
                break;
            case "ParentCategory.IsActive":
                if (entity.ParentCategory != null)
                {
                    result.ParentCategory ??= new CategoryProjection();
                    result.ParentCategory.IsActive = entity.ParentCategory.IsActive;
                }
                break;
            case "ParentCategory.CreatedAt":
                if (entity.ParentCategory != null)
                {
                    result.ParentCategory ??= new CategoryProjection();
                    result.ParentCategory.CreatedAt = entity.ParentCategory.CreatedAt;
                }
                break;
            case "ParentCategory.UpdatedAt":
                if (entity.ParentCategory != null)
                {
                    result.ParentCategory ??= new CategoryProjection();
                    result.ParentCategory.UpdatedAt = entity.ParentCategory.UpdatedAt;
                }
                break;
        }
    }
}
