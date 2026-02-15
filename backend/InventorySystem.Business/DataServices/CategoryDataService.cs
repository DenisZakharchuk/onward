using Inventorization.Base.Abstractions;
using Inventorization.Base.DTOs;
using InventorySystem.Business.Abstractions;
using InventorySystem.Business.Abstractions.Services;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Category;

namespace InventorySystem.Business.DataServices;

/// <summary>
/// Category data service implementing IDataService pattern
/// </summary>
public class CategoryDataService : ICategoryService
{
    private readonly InventorySystem.DataAccess.Abstractions.IUnitOfWork _unitOfWork;
    private readonly IMapper<Category, CategoryDetailsDTO> _mapper;
    private readonly IEntityCreator<Category, CreateCategoryDTO> _creator;
    private readonly IEntityModifier<Category, UpdateCategoryDTO> _modifier;
    private readonly ISearchQueryProvider<Category, CategorySearchDTO> _searchProvider;
    private readonly IValidator<CreateCategoryDTO> _createValidator;
    private readonly IValidator<UpdateCategoryDTO> _updateValidator;
    private readonly IAuditLogger? _auditLogger;

    public CategoryDataService(
        InventorySystem.DataAccess.Abstractions.IUnitOfWork unitOfWork,
        IMapper<Category, CategoryDetailsDTO> mapper,
        IEntityCreator<Category, CreateCategoryDTO> creator,
        IEntityModifier<Category, UpdateCategoryDTO> modifier,
        ISearchQueryProvider<Category, CategorySearchDTO> searchProvider,
        IValidator<CreateCategoryDTO> createValidator,
        IValidator<UpdateCategoryDTO> updateValidator,
        IAuditLogger? auditLogger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _creator = creator;
        _modifier = modifier;
        _searchProvider = searchProvider;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _auditLogger = auditLogger;
    }

    public async Task<ServiceResult<CategoryDetailsDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
            if (category == null)
                return ServiceResult<CategoryDetailsDTO>.Failure("Category not found");

            return ServiceResult<CategoryDetailsDTO>.Success(_mapper.Map(category));
        }
        catch (Exception ex)
        {
            return ServiceResult<CategoryDetailsDTO>.Failure($"Error retrieving category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CategoryDetailsDTO>> AddAsync(CreateCategoryDTO createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = await _createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validation.IsValid)
                return ServiceResult<CategoryDetailsDTO>.Failure("Validation failed", validation.Errors);

            var category = _creator.Create(createDto);
            await _unitOfWork.Categories.CreateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _ = LogAuditAsync("CategoryCreated", "Category", category.Id.ToString(), 
                new Dictionary<string, object> { { "name", category.Name } });

            return ServiceResult<CategoryDetailsDTO>.Success(_mapper.Map(category), "Category created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<CategoryDetailsDTO>.Failure($"Error creating category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CategoryDetailsDTO>> UpdateAsync(UpdateCategoryDTO updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = await _updateValidator.ValidateAsync(updateDto, cancellationToken);
            if (!validation.IsValid)
                return ServiceResult<CategoryDetailsDTO>.Failure("Validation failed", validation.Errors);

            var existing = await _unitOfWork.Categories.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existing == null)
                return ServiceResult<CategoryDetailsDTO>.Failure("Category not found");

            _modifier.Modify(existing, updateDto);
            await _unitOfWork.Categories.UpdateAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _ = LogAuditAsync("CategoryUpdated", "Category", updateDto.Id.ToString(), 
                new Dictionary<string, object> { { "name", existing.Name } });

            return ServiceResult<CategoryDetailsDTO>.Success(_mapper.Map(existing), "Category updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<CategoryDetailsDTO>.Failure($"Error updating category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(DeleteDTO deleteDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(deleteDto.Id, cancellationToken);
            if (category == null)
                return ServiceResult<bool>.Failure("Category not found");

            var result = await _unitOfWork.Categories.DeleteAsync(deleteDto.Id, cancellationToken);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _ = LogAuditAsync("CategoryDeleted", "Category", deleteDto.Id.ToString(), 
                    new Dictionary<string, object> { { "name", category.Name } });
            }
            
            return ServiceResult<bool>.Success(result, "Category deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"Error deleting category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CategoryDetailsDTO>> InitAsync(InitCategoryDTO initDto, CancellationToken cancellationToken = default)
    {
        if (initDto == null)
            return ServiceResult<CategoryDetailsDTO>.Failure("Category init data is required");

        return await GetByIdAsync(initDto.Id, cancellationToken);
    }

    public async Task<ServiceResult<PagedResult<CategoryDetailsDTO>>> SearchAsync(CategorySearchDTO searchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var allCategories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
            
            var searchExpression = _searchProvider.GetSearchExpression(searchDto);
            var searchFunc = searchExpression.Compile();
            var filtered = allCategories.Where(searchFunc).ToList();

            var page = searchDto.Page ?? new PageDTO { PageNumber = 1, PageSize = 10 };
            var totalCount = filtered.Count;
            
            var items = filtered
                .Skip((page.PageNumber - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToList();

            var dtos = items.Select(_mapper.Map).ToList();

            var result = new PagedResult<CategoryDetailsDTO>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            };

            return ServiceResult<PagedResult<CategoryDetailsDTO>>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<CategoryDetailsDTO>>.Failure($"Error searching categories: {ex.Message}");
        }
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
}
