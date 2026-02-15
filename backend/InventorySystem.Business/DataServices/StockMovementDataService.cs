using Inventorization.Base.Abstractions;
using Inventorization.Base.DTOs;
using InventorySystem.Business.Abstractions;
using InventorySystem.Business.Abstractions.Services;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.StockMovement;

namespace InventorySystem.Business.DataServices;

/// <summary>
/// StockMovement data service implementing IDataService pattern
/// </summary>
public class StockMovementDataService : IStockMovementService
{
    private readonly InventorySystem.DataAccess.Abstractions.IUnitOfWork _unitOfWork;
    private readonly IMapper<StockMovement, StockMovementDetailsDTO> _mapper;
    private readonly IEntityCreator<StockMovement, CreateStockMovementDTO> _creator;
    private readonly ISearchQueryProvider<StockMovement, StockMovementSearchDTO> _searchProvider;
    private readonly IValidator<CreateStockMovementDTO> _createValidator;
    private readonly IAuditLogger? _auditLogger;

    public StockMovementDataService(
        InventorySystem.DataAccess.Abstractions.IUnitOfWork unitOfWork,
        IMapper<StockMovement, StockMovementDetailsDTO> mapper,
        IEntityCreator<StockMovement, CreateStockMovementDTO> creator,
        ISearchQueryProvider<StockMovement, StockMovementSearchDTO> searchProvider,
        IValidator<CreateStockMovementDTO> createValidator,
        IAuditLogger? auditLogger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _creator = creator;
        _searchProvider = searchProvider;
        _createValidator = createValidator;
        _auditLogger = auditLogger;
    }

    public async Task<ServiceResult<StockMovementDetailsDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var movement = await _unitOfWork.StockMovements.GetByIdAsync(id, cancellationToken);
            if (movement == null)
                return ServiceResult<StockMovementDetailsDTO>.Failure("Stock movement not found");

            return ServiceResult<StockMovementDetailsDTO>.Success(_mapper.Map(movement));
        }
        catch (Exception ex)
        {
            return ServiceResult<StockMovementDetailsDTO>.Failure($"Error retrieving stock movement: {ex.Message}");
        }
    }

    public async Task<ServiceResult<StockMovementDetailsDTO>> AddAsync(CreateStockMovementDTO createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = await _createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validation.IsValid)
                return ServiceResult<StockMovementDetailsDTO>.Failure("Validation failed", validation.Errors);

            var movement = _creator.Create(createDto);
            await _unitOfWork.StockMovements.CreateAsync(movement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _ = LogAuditAsync("StockMovementCreated", "StockMovement", movement.Id.ToString(), 
                new Dictionary<string, object> { { "quantity", movement.Quantity }, { "type", movement.Type } });

            return ServiceResult<StockMovementDetailsDTO>.Success(_mapper.Map(movement), "Stock movement created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<StockMovementDetailsDTO>.Failure($"Error creating stock movement: {ex.Message}");
        }
    }

    public async Task<ServiceResult<StockMovementDetailsDTO>> UpdateAsync(UpdateDTO updateDto, CancellationToken cancellationToken = default)
    {
        // Stock movements are immutable, so update is not supported
        return await Task.FromResult(ServiceResult<StockMovementDetailsDTO>.Failure("Stock movements cannot be updated"));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(DeleteDTO deleteDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var movement = await _unitOfWork.StockMovements.GetByIdAsync(deleteDto.Id, cancellationToken);
            if (movement == null)
                return ServiceResult<bool>.Failure("Stock movement not found");

            var result = await _unitOfWork.StockMovements.DeleteAsync(deleteDto.Id, cancellationToken);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _ = LogAuditAsync("StockMovementDeleted", "StockMovement", deleteDto.Id.ToString(), 
                    new Dictionary<string, object> { { "productId", movement.ProductId } });
            }
            
            return ServiceResult<bool>.Success(result, "Stock movement deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"Error deleting stock movement: {ex.Message}");
        }
    }

    public async Task<ServiceResult<StockMovementDetailsDTO>> InitAsync(InitStockMovementDTO initDto, CancellationToken cancellationToken = default)
    {
        if (initDto == null)
            return ServiceResult<StockMovementDetailsDTO>.Failure("Stock movement init data is required");

        return await GetByIdAsync(initDto.Id, cancellationToken);
    }

    public async Task<ServiceResult<PagedResult<StockMovementDetailsDTO>>> SearchAsync(StockMovementSearchDTO searchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var allMovements = await _unitOfWork.StockMovements.GetAllAsync(cancellationToken);
            
            var searchExpression = _searchProvider.GetSearchExpression(searchDto);
            var searchFunc = searchExpression.Compile();
            var filtered = allMovements.Where(searchFunc).ToList();

            var page = searchDto.Page ?? new PageDTO { PageNumber = 1, PageSize = 10 };
            var totalCount = filtered.Count;
            
            var items = filtered
                .Skip((page.PageNumber - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToList();

            var dtos = items.Select(_mapper.Map).ToList();

            var result = new PagedResult<StockMovementDetailsDTO>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            };

            return ServiceResult<PagedResult<StockMovementDetailsDTO>>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<StockMovementDetailsDTO>>.Failure($"Error searching stock movements: {ex.Message}");
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
