using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventorization.Base.Services;

// Use fully-qualified IUnitOfWork from DataAccess to avoid ambiguity
using IUnitOfWorkInterface = Inventorization.Base.DataAccess.IUnitOfWork;

/// <summary>
/// Abstract base class for generic data services implementing full CRUD operations
/// </summary>
/// <remarks>
/// This class reduces boilerplate code by providing common implementation for all data services.
/// Dependencies are resolved lazily from IServiceProvider to minimize overhead.
/// Subclasses only need to define the interface and constructor call.
/// 
/// This is a reusable service abstraction for all bounded contexts.
/// </remarks>
public abstract class DataServiceBase<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>
    : IDataService<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : UpdateDTO
    where TDeleteDTO : DeleteDTO
    where TDetailsDTO : class
    where TSearchDTO : class
{
    protected readonly IUnitOfWorkInterface UnitOfWork;
    protected readonly IRepository<TEntity> Repository;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger<DataServiceBase<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>> Logger;

    protected string EntityName => typeof(TEntity).Name;

    public DataServiceBase(
        IUnitOfWorkInterface unitOfWork,
        IRepository<TEntity> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>> logger)
    {
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a single entity by ID
    /// </summary>
    public async Task<ServiceResult<TDetailsDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id == Guid.Empty)
                return ServiceResult<TDetailsDTO>.Failure($"{EntityName} ID is required");

            var entity = await Repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ServiceResult<TDetailsDTO>.Failure($"{EntityName} not found");

            var mapper = ServiceProvider.GetRequiredService<IMapper<TEntity, TDetailsDTO>>();
            return ServiceResult<TDetailsDTO>.Success(mapper.Map(entity));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting {EntityName} {Id}", EntityName, id);
            return ServiceResult<TDetailsDTO>.Failure($"Failed to get {EntityName.ToLower()}");
        }
    }

    /// <summary>
    /// Creates a new entity
    /// </summary>
    public async Task<ServiceResult<TDetailsDTO>> AddAsync(TCreateDTO createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (createDto == null)
                return ServiceResult<TDetailsDTO>.Failure($"{EntityName} data is required");

            var createValidator = ServiceProvider.GetRequiredService<IValidator<TCreateDTO>>();
            var validationResult = await createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validationResult.IsValid)
                return ServiceResult<TDetailsDTO>.Failure(validationResult.Errors);

            var creator = ServiceProvider.GetRequiredService<IEntityCreator<TEntity, TCreateDTO>>();
            var entity = creator.Create(createDto);

            await Repository.CreateAsync(entity, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            var mapper = ServiceProvider.GetRequiredService<IMapper<TEntity, TDetailsDTO>>();
            Logger.LogInformation("{EntityName} created successfully: {EntityId}", EntityName, GetEntityId(entity));
            return ServiceResult<TDetailsDTO>.Success(mapper.Map(entity), $"{EntityName} created successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating {EntityName}", EntityName);
            return ServiceResult<TDetailsDTO>.Failure($"Failed to create {EntityName.ToLower()}");
        }
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    public async Task<ServiceResult<TDetailsDTO>> UpdateAsync(TUpdateDTO updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (updateDto == null)
                return ServiceResult<TDetailsDTO>.Failure($"{EntityName} data is required");

            var updateValidator = ServiceProvider.GetRequiredService<IValidator<TUpdateDTO>>();
            var validationResult = await updateValidator.ValidateAsync(updateDto, cancellationToken);
            if (!validationResult.IsValid)
                return ServiceResult<TDetailsDTO>.Failure(validationResult.Errors);

            var entity = await Repository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (entity == null)
                return ServiceResult<TDetailsDTO>.Failure($"{EntityName} not found");

            var modifier = ServiceProvider.GetRequiredService<IEntityModifier<TEntity, TUpdateDTO>>();
            modifier.Modify(entity, updateDto);
            
            await Repository.UpdateAsync(entity, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            var mapper = ServiceProvider.GetRequiredService<IMapper<TEntity, TDetailsDTO>>();
            Logger.LogInformation("{EntityName} updated successfully: {EntityId}", EntityName, updateDto.Id);
            return ServiceResult<TDetailsDTO>.Success(mapper.Map(entity), $"{EntityName} updated successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating {EntityName}", EntityName);
            return ServiceResult<TDetailsDTO>.Failure($"Failed to update {EntityName.ToLower()}");
        }
    }

    /// <summary>
    /// Deletes an entity
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(TDeleteDTO deleteDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (deleteDto == null)
                return ServiceResult<bool>.Failure("Delete request is required");

            var deleted = await Repository.DeleteAsync(deleteDto.Id, cancellationToken);
            if (!deleted)
                return ServiceResult<bool>.Failure($"{EntityName} not found");

            await UnitOfWork.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("{EntityName} deleted successfully: {EntityId}", EntityName, deleteDto.Id);
            return ServiceResult<bool>.Success(true, $"{EntityName} deleted successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting {EntityName}", EntityName);
            return ServiceResult<bool>.Failure($"Failed to delete {EntityName.ToLower()}");
        }
    }

    /// <summary>
    /// Searches for entities with pagination
    /// </summary>
    public async Task<ServiceResult<PagedResult<TDetailsDTO>>> SearchAsync(TSearchDTO searchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (searchDto == null)
                searchDto = Activator.CreateInstance<TSearchDTO>();

            var searchProvider = ServiceProvider.GetRequiredService<ISearchQueryProvider<TEntity, TSearchDTO>>();
            var filter = searchProvider.GetSearchExpression(searchDto);
            var query = Repository.GetQueryable().Where(filter);

            var total = await query.CountAsync(cancellationToken);
            
            // Handle pagination - get Page property from searchDto
            var pageProperty = typeof(TSearchDTO).GetProperty("Page");
            var page = pageProperty?.GetValue(searchDto) as PageDTO ?? new PageDTO();

            // Load entities with pagination, then project with pre-sized list
            var entities = await query
                .Skip((page.PageNumber - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToListAsync(cancellationToken);
            
            var mapper = ServiceProvider.GetRequiredService<IMapper<TEntity, TDetailsDTO>>();
            var projectionFunc = mapper.GetProjection().Compile();
            var items = new List<TDetailsDTO>(entities.Count);
            foreach (var entity in entities)
                items.Add(projectionFunc(entity));

            return ServiceResult<PagedResult<TDetailsDTO>>.Success(
                new PagedResult<TDetailsDTO>
                {
                    Items = items,
                    TotalCount = total,
                    PageNumber = page.PageNumber,
                    PageSize = page.PageSize
                }
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching {EntityName}", EntityName);
            return ServiceResult<PagedResult<TDetailsDTO>>.Failure($"Failed to search {EntityName.ToLower()}");
        }
    }

    /// <summary>
    /// Helper method to get entity ID for logging
    /// </summary>
    protected virtual object? GetEntityId(TEntity entity)
    {
        var idProperty = typeof(TEntity).GetProperty("Id");
        return idProperty?.GetValue(entity);
    }
}
