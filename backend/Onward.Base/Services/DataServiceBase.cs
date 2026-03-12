using Onward.Base.Abstractions;
using Onward.Base.DataAccess;
using Onward.Base.DTOs;
using Onward.Base.Models;
using Onward.Base.Ownership;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Onward.Base.Services;

// Use fully-qualified IUnitOfWork from DataAccess to avoid ambiguity
using IUnitOfWorkInterface = Onward.Base.DataAccess.IUnitOfWork;

/// <summary>
/// Abstract base class for generic data services implementing full CRUD operations,
/// with optional ownership stamping via <see cref="ICurrentIdentityContext{TOwnership}"/>.
/// </summary>
/// <remarks>
/// When <typeparamref name="TOwnership"/> is provided and the entity implements
/// <see cref="IOwnedEntity{TOwnership}"/>, the service automatically stamps
/// <see cref="IOwnedEntity{TOwnership}.Ownership"/> on creation and
/// <see cref="IOwnedEntity{TOwnership}.LastModifiedOwnership"/> on update.
///
/// <typeparamref name="TOwnership"/> is constrained to <see cref="OwnershipValueObject"/>
/// so the service is decoupled from any specific identity shape.
///
/// For entities that do not require ownership tracking, derive from
/// <see cref="DataServiceBase{TEntity,TCreateDTO,TUpdateDTO,TDeleteDTO,TInitDTO,TDetailsDTO,TSearchDTO}"/>
/// instead (the non-generic variant below).
/// </remarks>
public abstract class DataServiceBase<TOwnership, TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, TKey>
    : IDataService<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, TKey>
    where TOwnership : OwnershipValueObject
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : UpdateDTO<TKey>
    where TDeleteDTO : DeleteDTO<TKey>
    where TInitDTO : InitDTO<TKey>
    where TDetailsDTO : class
    where TSearchDTO : class
{
    protected readonly IUnitOfWorkInterface UnitOfWork;
    protected readonly IRepository<TEntity, TKey> Repository;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ICurrentIdentityContext<TOwnership> IdentityContext;
    protected readonly ILogger<DataServiceBase<TOwnership, TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, TKey>> Logger;

    protected string EntityName => typeof(TEntity).Name;

    protected DataServiceBase(
        IUnitOfWorkInterface unitOfWork,
        IRepository<TEntity, TKey> repository,
        IServiceProvider serviceProvider,
        ICurrentIdentityContext<TOwnership> identityContext,
        ILogger<DataServiceBase<TOwnership, TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, TKey>> logger)
    {
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        IdentityContext = identityContext ?? throw new ArgumentNullException(nameof(identityContext));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Stamps ownership on the entity if it implements <see cref="IOwnedEntity{TOwnership}"/>
    /// and the caller is authenticated.  Returns a failure result when ownership is
    /// required but the caller is anonymous.
    /// </summary>
    private ServiceResult<TDetailsDTO>? TryStampCreateOwnership(TEntity entity)
    {
        if (entity is not IOwnedEntity<TOwnership> owned)
            return null; // entity does not participate in ownership — skip silently

        if (!IdentityContext.IsAuthenticated || IdentityContext.Ownership is null)
        {
            Logger.LogWarning("Attempted to create {EntityName} without an authenticated identity", EntityName);
            return ServiceResult<TDetailsDTO>.Failure($"An authenticated identity is required to create {EntityName}");
        }

        owned.SetOwnership(IdentityContext.Ownership);
        return null; // null = success, proceed
    }

    /// <summary>
    /// Stamps last-modified ownership on the entity if it implements
    /// <see cref="IOwnedEntity{TOwnership}"/> and the caller is authenticated.
    /// A missing identity is logged as a warning but does not block the update.
    /// </summary>
    private void TryStampUpdateOwnership(TEntity entity)
    {
        if (entity is not IOwnedEntity<TOwnership> owned)
            return;

        if (!IdentityContext.IsAuthenticated || IdentityContext.Ownership is null)
        {
            Logger.LogWarning("Updating {EntityName} without an authenticated identity — LastModifiedOwnership will not be stamped", EntityName);
            return;
        }

        owned.UpdateOwnership(IdentityContext.Ownership);
    }

    /// <summary>
    /// Gets a single entity by ID. Integrates with idempotency caching and ETag generation.
    /// </summary>
    public async Task<ServiceResult<TDetailsDTO>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsDefaultKey(id))
                return ServiceResult<TDetailsDTO>.Failure($"{EntityName} ID is required");

            var tokenAccessor = ServiceProvider.GetService<IIdempotencyTokenAccessor>()
                                ?? NoOpIdempotencyTokenAccessor.Instance;
            var responseCache  = ServiceProvider.GetService<IResponseCacheContext>()
                                ?? NoOpResponseCacheContext.Instance;

            // Check conditional GET (If-None-Match) before hitting the DB
            var conditionalToken = tokenAccessor.GetConditionalToken();

            // Try response cache first
            var cacheKey = $"{EntityName}:{id}";
            var (cacheHit, cached, cachedToken) = await responseCache.TryGetAsync<TDetailsDTO>(cacheKey, cancellationToken);
            if (cacheHit && cached is not null)
            {
                if (conditionalToken is not null && conditionalToken == cachedToken)
                    return ServiceResult<TDetailsDTO>.NotModified();

                if (cachedToken is not null)
                    tokenAccessor.SetResponseToken(cachedToken);

                return ServiceResult<TDetailsDTO>.Success(cached);
            }

            var entity = await Repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ServiceResult<TDetailsDTO>.Failure($"{EntityName} not found");

            var versionToken = (entity is IVersionedEntity v) ? v.RowVersion.ToString() : null;

            // 304 short-circuit from DB token
            if (conditionalToken is not null && conditionalToken == versionToken)
                return ServiceResult<TDetailsDTO>.NotModified();

            var mapper = ServiceProvider.GetRequiredService<IMapper<TEntity, TDetailsDTO>>();
            var dto = mapper.Map(entity);

            // Store in response cache
            await responseCache.SetAsync(cacheKey, dto, versionToken, cancellationToken);

            if (versionToken is not null)
                tokenAccessor.SetResponseToken(versionToken);

            return ServiceResult<TDetailsDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting {EntityName} {Id}", EntityName, id);
            return ServiceResult<TDetailsDTO>.Failure($"Failed to get {EntityName.ToLower()}");
        }
    }

    /// <summary>
    /// Creates a new entity. When the entity implements <see cref="IOwnedEntity{TOwnership}"/>,
    /// ownership is stamped from the current <see cref="ICurrentIdentityContext{TOwnership}"/>
    /// before the entity is persisted. Supports POST idempotency via X-Idempotency-Key header.
    /// </summary>
    public async Task<ServiceResult<TDetailsDTO>> AddAsync(TCreateDTO createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (createDto == null)
                return ServiceResult<TDetailsDTO>.Failure($"{EntityName} data is required");

            var tokenAccessor = ServiceProvider.GetService<IIdempotencyTokenAccessor>()
                                ?? NoOpIdempotencyTokenAccessor.Instance;
            var responseCache  = ServiceProvider.GetService<IResponseCacheContext>()
                                ?? NoOpResponseCacheContext.Instance;

            // POST deduplication: if client sent X-Idempotency-Key and we have a cached result, return it
            var idempotencyKey = tokenAccessor.GetIdempotencyKey();
            if (idempotencyKey is not null)
            {
                var (hit, cached, cachedToken) = await responseCache.TryGetAsync<TDetailsDTO>($"post:{idempotencyKey}", cancellationToken);
                if (hit && cached is not null)
                {
                    if (cachedToken is not null) tokenAccessor.SetResponseToken(cachedToken);
                    return ServiceResult<TDetailsDTO>.Success(cached, $"{EntityName} already created (idempotent)");
                }
            }

            var createValidator = ServiceProvider.GetRequiredService<IValidator<TCreateDTO>>();
            var validationResult = await createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validationResult.IsValid)
                return ServiceResult<TDetailsDTO>.Failure(validationResult.Errors);

            var creator = ServiceProvider.GetRequiredService<IEntityCreator<TEntity, TCreateDTO>>();
            var entity = creator.Create(createDto);

            // Stamp ownership before persistence if entity participates in ownership
            var ownershipError = TryStampCreateOwnership(entity);
            if (ownershipError is not null)
                return ownershipError;

            await Repository.CreateAsync(entity, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            var mapper = ServiceProvider.GetRequiredService<IMapper<TEntity, TDetailsDTO>>();
            var dto = mapper.Map(entity);
            var versionToken = (entity is IVersionedEntity v) ? v.RowVersion.ToString() : null;

            // Cache the result under X-Idempotency-Key for safe retries
            if (idempotencyKey is not null)
                await responseCache.SetAsync($"post:{idempotencyKey}", dto, versionToken, cancellationToken);

            // Also prime the GET cache
            var entityId = GetEntityId(entity);
            if (entityId is not null)
                await responseCache.SetAsync($"{EntityName}:{entityId}", dto, versionToken, cancellationToken);

            if (versionToken is not null)
                tokenAccessor.SetResponseToken(versionToken);

            Logger.LogInformation("{EntityName} created successfully: {EntityId}", EntityName, entityId);
            return ServiceResult<TDetailsDTO>.Success(dto, $"{EntityName} created successfully");
        }
        catch (DbUpdateException dbEx) when (IsUniqueViolation(dbEx))
        {
            Logger.LogWarning(dbEx, "Unique constraint violation creating {EntityName}", EntityName);
            return ServiceResult<TDetailsDTO>.Conflict(
                $"{EntityName} already exists with the same unique field values.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating {EntityName}", EntityName);
            return ServiceResult<TDetailsDTO>.Failure($"Failed to create {EntityName.ToLower()}");
        }
    }

    /// <summary>
    /// Updates an existing entity. When the entity implements <see cref="IOwnedEntity{TOwnership}"/>,
    /// <see cref="IOwnedEntity{TOwnership}.LastModifiedOwnership"/> is stamped from the current identity.
    /// When the entity implements <see cref="IVersionedEntity"/>, the If-Match token from the HTTP
    /// request is checked via <see cref="IIdempotencyTokenAccessor"/>; a stale token returns 409.
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

            // Optimistic concurrency check (only for versioned entities)
            if (entity is IVersionedEntity versioned)
            {
                var tokenAccessor = ServiceProvider.GetService<IIdempotencyTokenAccessor>()
                                    ?? NoOpIdempotencyTokenAccessor.Instance;
                var clientToken = tokenAccessor.GetMutationToken();
                if (clientToken is not null && clientToken != versioned.RowVersion.ToString())
                    return ServiceResult<TDetailsDTO>.Conflict(
                        $"{EntityName} was modified since you last read it. Reload and retry.");
            }

            var modifier = ServiceProvider.GetRequiredService<IEntityModifier<TEntity, TUpdateDTO>>();
            modifier.Modify(entity, updateDto);

            // Stamp last-modified ownership if entity participates in ownership
            TryStampUpdateOwnership(entity);

            await Repository.UpdateAsync(entity, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            var mapper = ServiceProvider.GetRequiredService<IMapper<TEntity, TDetailsDTO>>();
            var dto = mapper.Map(entity);
            var versionToken = (entity is IVersionedEntity v) ? v.RowVersion.ToString() : null;

            // Refresh caches
            var responseCache = ServiceProvider.GetService<IResponseCacheContext>()
                                ?? NoOpResponseCacheContext.Instance;
            await responseCache.SetAsync($"{EntityName}:{updateDto.Id}", dto, versionToken, cancellationToken);

            var tokenAcc = ServiceProvider.GetService<IIdempotencyTokenAccessor>()
                           ?? NoOpIdempotencyTokenAccessor.Instance;
            if (versionToken is not null) tokenAcc.SetResponseToken(versionToken);

            Logger.LogInformation("{EntityName} updated successfully: {EntityId}", EntityName, updateDto.Id);
            return ServiceResult<TDetailsDTO>.Success(dto, $"{EntityName} updated successfully");
        }
        catch (DbUpdateConcurrencyException dbConcEx)
        {
            Logger.LogWarning(dbConcEx, "Concurrency conflict updating {EntityName}", EntityName);
            return ServiceResult<TDetailsDTO>.Conflict(
                $"{EntityName} was modified concurrently. Reload and retry.");
        }
        catch (DbUpdateException dbEx) when (IsUniqueViolation(dbEx))
        {
            Logger.LogWarning(dbEx, "Unique constraint violation updating {EntityName}", EntityName);
            return ServiceResult<TDetailsDTO>.Conflict(
                $"{EntityName} already exists with the same unique field values.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating {EntityName}", EntityName);
            return ServiceResult<TDetailsDTO>.Failure($"Failed to update {EntityName.ToLower()}");
        }
    }

    /// <summary>
    /// Deletes an entity. When the entity implements <see cref="IVersionedEntity"/>, the If-Match
    /// token from the HTTP request is checked via <see cref="IIdempotencyTokenAccessor"/>;
    /// a stale token returns 409.
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(TDeleteDTO deleteDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (deleteDto == null)
                return ServiceResult<bool>.Failure("Delete request is required");

            // Load entity first so we can (a) 404-check and (b) do a versioned delete
            var entity = await Repository.GetByIdAsync(deleteDto.Id, cancellationToken);
            if (entity == null)
                return ServiceResult<bool>.Failure($"{EntityName} not found");

            // Optimistic concurrency check (only for versioned entities)
            if (entity is IVersionedEntity versioned)
            {
                var tokenAccessor = ServiceProvider.GetService<IIdempotencyTokenAccessor>()
                                    ?? NoOpIdempotencyTokenAccessor.Instance;
                var clientToken = tokenAccessor.GetMutationToken();
                if (clientToken is not null && clientToken != versioned.RowVersion.ToString())
                    return ServiceResult<bool>.Conflict(
                        $"{EntityName} was modified since you last read it. Reload and retry.");
            }

            // Use entity-based delete so EF Core includes the xmin token in the WHERE clause
            await Repository.DeleteAsync(entity, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            // Evict from response cache
            var responseCache = ServiceProvider.GetService<IResponseCacheContext>()
                                ?? NoOpResponseCacheContext.Instance;
            await responseCache.SetAsync($"{EntityName}:{deleteDto.Id}", (object?)null, null, cancellationToken);

            Logger.LogInformation("{EntityName} deleted successfully: {EntityId}", EntityName, deleteDto.Id);
            return ServiceResult<bool>.Success(true, $"{EntityName} deleted successfully");
        }
        catch (DbUpdateConcurrencyException dbConcEx)
        {
            Logger.LogWarning(dbConcEx, "Concurrency conflict deleting {EntityName}", EntityName);
            return ServiceResult<bool>.Conflict(
                $"{EntityName} was modified concurrently. Reload and retry.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting {EntityName}", EntityName);
            return ServiceResult<bool>.Failure($"Failed to delete {EntityName.ToLower()}");
        }
    }

    /// <summary>
    /// Initializes entity details for CRUD scenarios using minimal init payload.
    /// Default implementation loads details by ID.
    /// </summary>
    public virtual async Task<ServiceResult<TDetailsDTO>> InitAsync(TInitDTO initDto, CancellationToken cancellationToken = default)
    {
        if (initDto == null)
            return ServiceResult<TDetailsDTO>.Failure($"{EntityName} init data is required");

        return await GetByIdAsync(initDto.Id, cancellationToken);
    }

    /// <summary>
    /// Applies <see cref="ITenantScopeFilter{TEntity}"/> to the query when:
    /// <list type="number">
    ///   <item>An <see cref="ITenantContext"/> is registered in the service provider, AND</item>
    ///   <item><see cref="ITenantContext.CurrentTenantId"/> is non-null, AND</item>
    ///   <item>An <see cref="ITenantScopeFilter{TEntity}"/> implementation is registered.</item>
    /// </list>
    /// Otherwise the query is returned unchanged.
    /// </summary>
    protected IQueryable<TEntity> ApplyTenantScope(IQueryable<TEntity> query)
    {
        var tenantContext = ServiceProvider.GetService<ITenantContext>();
        var tenantId = tenantContext?.CurrentTenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
            return query;

        var filter = ServiceProvider.GetService<ITenantScopeFilter<TEntity>>();
        return filter is not null ? filter.Apply(query, tenantId) : query;
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
            var query = ApplyTenantScope(Repository.GetQueryable()).Where(filter);

            var total = await query.CountAsync(cancellationToken);
            
            // Handle pagination - get Page property from searchDto
            var pageProperty = typeof(TSearchDTO).GetProperty("Page");
            var page = pageProperty?.GetValue(searchDto) as PageDTO ?? new PageDTO();

            var mapper = ServiceProvider.GetRequiredService<IMapper<TEntity, TDetailsDTO>>();
            var items = await query
                .Skip((page.PageNumber - 1) * page.PageSize)
                .Take(page.PageSize)
                .Select(mapper.GetProjection())
                .ToListAsync(cancellationToken);

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
    /// Determines whether the given key is the default (empty) value.
    /// Override for custom key types (e.g., <see langword="string"/>).
    /// </summary>
    protected virtual bool IsDefaultKey(TKey id)
        => EqualityComparer<TKey>.Default.Equals(id, default!);

    /// <summary>
    /// Helper method to get entity ID for logging
    /// </summary>
    protected virtual object? GetEntityId(TEntity entity)
    {
        var idProperty = typeof(TEntity).GetProperty("Id");
        return idProperty?.GetValue(entity);
    }

    /// <summary>
    /// Returns true when <paramref name="ex"/> is caused by a unique constraint violation.
    /// Checks the inner exception message for common database error strings without
    /// requiring a direct dependency on the database provider package (e.g. Npgsql).
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true
        || ex.InnerException?.Message.Contains("violates unique constraint", StringComparison.OrdinalIgnoreCase) == true
        || ex.InnerException?.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) == true;
}

/// <summary>
/// Ownership-aware base class for Guid-primary-key entities — convenience alias.
/// </summary>
public abstract class DataServiceBase<TOwnership, TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO>
    : DataServiceBase<TOwnership, TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, Guid>
    where TOwnership : OwnershipValueObject
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : UpdateDTO<Guid>
    where TDeleteDTO : DeleteDTO<Guid>
    where TInitDTO : InitDTO<Guid>
    where TDetailsDTO : class
    where TSearchDTO : class
{
    protected DataServiceBase(
        IUnitOfWorkInterface unitOfWork,
        IRepository<TEntity, Guid> repository,
        IServiceProvider serviceProvider,
        ICurrentIdentityContext<TOwnership> identityContext,
        ILogger<DataServiceBase<TOwnership, TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, Guid>> logger)
        : base(unitOfWork, repository, serviceProvider, identityContext, logger)
    {
    }
}

/// <summary>
/// Backward-compatible, non-ownership variant of <see cref="DataServiceBase{TOwnership,TEntity,TCreateDTO,TUpdateDTO,TDeleteDTO,TInitDTO,TDetailsDTO,TSearchDTO}"/>.
/// Use this as the base class for data services whose entities do not implement
/// <see cref="IOwnedEntity{TOwnership}"/> and therefore require no ownership stamping.
/// <para>
/// Internally delegates to the ownership-aware base with an
/// <see cref="AnonymousIdentityContext{TOwnership}"/> that is never accessed
/// because no entity in this path implements <see cref="IOwnedEntity{TOwnership}"/>.
/// </para>
/// </summary>
public abstract class DataServiceBase<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO>
    : DataServiceBase<NoOwnership, TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, Guid>
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : UpdateDTO<Guid>
    where TDeleteDTO : DeleteDTO<Guid>
    where TInitDTO : InitDTO<Guid>
    where TDetailsDTO : class
    where TSearchDTO : class
{
    protected DataServiceBase(
        IUnitOfWorkInterface unitOfWork,
        IRepository<TEntity, Guid> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider,
               AnonymousIdentityContext<NoOwnership>.Instance,
               // Logger type coercion: wrap in a typed forwarder so base class logger type param matches
               new TypeForwardingLogger<DataServiceBase<NoOwnership, TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TInitDTO, TDetailsDTO, TSearchDTO, Guid>>(logger))
    {
    }
}

/// <summary>
/// Sentinel ownership VO used by the non-ownership <see cref="DataServiceBase{TEntity,TCreateDTO,TUpdateDTO,TDeleteDTO,TInitDTO,TDetailsDTO,TSearchDTO}"/>.
/// It is never constructed at runtime — it only satisfies the generic constraint.
/// </summary>
public sealed record NoOwnership : OwnershipValueObject;

/// <summary>
/// Thin <see cref="ILogger{T}"/> wrapper that forwards all calls to an inner logger
/// while satisfying a different open-generic type parameter T.
/// Used to bridge the logger type parameter between the non-ownership shim and the
/// ownership-aware base class.
/// </summary>
internal sealed class TypeForwardingLogger<T> : ILogger<T>
{
    private readonly ILogger _inner;
    public TypeForwardingLogger(ILogger inner) => _inner = inner;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _inner.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => _inner.Log(logLevel, eventId, state, exception, formatter);
}
