using System.Linq.Expressions;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DTOs;
using InventorySystem.Business.Abstractions;
using InventorySystem.Business.Abstractions.Services;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;

namespace InventorySystem.Business.DataServices;

/// <summary>
/// Product data service implementing IDataService pattern
/// </summary>
public class ProductDataService : IProductService
{
    private readonly InventorySystem.DataAccess.Abstractions.IUnitOfWork _unitOfWork;
    private readonly IMapper<Product, ProductDetailsDTO> _mapper;
    private readonly IEntityCreator<Product, CreateProductDTO> _creator;
    private readonly IEntityModifier<Product, UpdateProductDTO> _modifier;
    private readonly ISearchQueryProvider<Product, ProductSearchDTO> _searchProvider;
    private readonly IValidator<CreateProductDTO> _createValidator;
    private readonly IValidator<UpdateProductDTO> _updateValidator;
    private readonly IAuditLogger? _auditLogger;

    public ProductDataService(
        InventorySystem.DataAccess.Abstractions.IUnitOfWork unitOfWork,
        IMapper<Product, ProductDetailsDTO> mapper,
        IEntityCreator<Product, CreateProductDTO> creator,
        IEntityModifier<Product, UpdateProductDTO> modifier,
        ISearchQueryProvider<Product, ProductSearchDTO> searchProvider,
        IValidator<CreateProductDTO> createValidator,
        IValidator<UpdateProductDTO> updateValidator,
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

    public async Task<ServiceResult<ProductDetailsDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return ServiceResult<ProductDetailsDTO>.Failure("Product not found");

            return ServiceResult<ProductDetailsDTO>.Success(_mapper.Map(product));
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDetailsDTO>.Failure($"Error retrieving product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductDetailsDTO>> AddAsync(CreateProductDTO createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = await _createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validation.IsValid)
                return ServiceResult<ProductDetailsDTO>.Failure("Validation failed", validation.Errors);

            var product = _creator.Create(createDto);
            await _unitOfWork.Products.CreateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log (fire-and-forget)
            _ = LogAuditAsync("ProductCreated", "Product", product.Id.ToString(), new Dictionary<string, object>
            {
                { "name", product.Name },
                { "price", product.Price },
                { "initialStock", product.CurrentStock }
            });

            return ServiceResult<ProductDetailsDTO>.Success(_mapper.Map(product), "Product created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDetailsDTO>.Failure($"Error creating product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductDetailsDTO>> UpdateAsync(UpdateProductDTO updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = await _updateValidator.ValidateAsync(updateDto, cancellationToken);
            if (!validation.IsValid)
                return ServiceResult<ProductDetailsDTO>.Failure("Validation failed", validation.Errors);

            var existing = await _unitOfWork.Products.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existing == null)
                return ServiceResult<ProductDetailsDTO>.Failure("Product not found");

            var changes = new Dictionary<string, object>();
            if (existing.Name != updateDto.Name) 
                changes["name"] = new { old = existing.Name, @new = updateDto.Name };
            if (existing.Price != updateDto.Price) 
                changes["price"] = new { old = existing.Price, @new = updateDto.Price };

            _modifier.Modify(existing, updateDto);
            
            await _unitOfWork.Products.UpdateAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            if (changes.Count > 0)
                _ = LogAuditAsync("ProductUpdated", "Product", updateDto.Id.ToString(), changes);

            return ServiceResult<ProductDetailsDTO>.Success(_mapper.Map(existing), "Product updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDetailsDTO>.Failure($"Error updating product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(DeleteDTO deleteDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(deleteDto.Id, cancellationToken);
            if (product == null)
                return ServiceResult<bool>.Failure("Product not found");

            var result = await _unitOfWork.Products.DeleteAsync(deleteDto.Id, cancellationToken);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                // Audit log
                _ = LogAuditAsync("ProductDeleted", "Product", deleteDto.Id.ToString(), new Dictionary<string, object>
                {
                    { "name", product.Name }
                });
            }
            
            return ServiceResult<bool>.Success(result, "Product deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"Error deleting product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ProductDetailsDTO>>> SearchAsync(ProductSearchDTO searchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var allProducts = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            
            // Apply search expression
            var searchExpression = _searchProvider.GetSearchExpression(searchDto);
            var searchFunc = searchExpression.Compile();
            var filtered = allProducts.Where(searchFunc).ToList();

            var page = searchDto.Page ?? new PageDTO { PageNumber = 1, PageSize = 10 };
            var totalCount = filtered.Count;
            
            var items = filtered
                .Skip((page.PageNumber - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToList();

            var dtos = items.Select(_mapper.Map).ToList();

            var result = new PagedResult<ProductDetailsDTO>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            };

            return ServiceResult<PagedResult<ProductDetailsDTO>>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<ProductDetailsDTO>>.Failure($"Error searching products: {ex.Message}");
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
