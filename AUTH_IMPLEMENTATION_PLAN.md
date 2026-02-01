# Auth Microservice Implementation Plan (Revised)

## Overview
Implement Auth microservice following standard Inventorization architecture patterns with IDataService, IMapper, IValidator abstractions for all entities.

---

## ✅ COMPLETION STATUS

### Phase 9: Data Access Layer - ✅ COMPLETE
- ✅ Base Repository (Inventorization.Base)
- ✅ Auth Repositories (4 total: User, Role, Permission, RefreshToken)
- ✅ Unit of Work (AuthUnitOfWork with lazy-loaded repositories)

### Phase 10: Mappers, Creators, Modifiers, Validators - ✅ COMPLETE
- ✅ 3 Mappers (User, Role, Permission) with GetProjection() method
- ✅ 3 Entity Creators with dependency-injected IPasswordHasher
- ✅ 3 Entity Modifiers with optional password update support
- ✅ 6 Validators (Create/Update for User, Role, Permission)
- ✅ 3 Search Query Providers with filter expressions
- ✅ **NEW**: IPasswordHasher interface added to Inventorization.Base

### Phase 11: Data Services - ✅ COMPLETE
- ✅ IUserDataService interface extending IDataService<...>
- ✅ UserDataService with full CRUD and search
- ✅ IRoleDataService interface and RoleDataService implementation
- ✅ IPermissionDataService interface and PermissionDataService implementation
- ✅ All services use constructor injection (removed IServiceProvider dependency)
- ✅ All services return ServiceResult<T> with error handling

### Build Status: 🎉 SUCCESS
- **0 Errors**
- 10 Warnings (acceptable: async without await, nullable reference guards)
- Ready for Phase 12: API Layer

---

## Phase 9: Data Access Layer

### 9.1 Base Repository (Inventorization.Base)
Extract to: `Inventorization.Base/DataAccess/BaseRepository.cs`
```csharp
public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    public BaseRepository(DbContext context) => _context = context;
    
    public async Task<T?> GetByIdAsync(Guid id) => await _context.Set<T>().FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();
    public async Task CreateAsync(T entity) => await _context.Set<T>().AddAsync(entity);
    public void Update(T entity) => _context.Set<T>().Update(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);
    public IQueryable<T> GetQueryable() => _context.Set<T>();
}
```

### 9.2 Auth Repositories
Create in: `Inventorization.Auth.Domain/DataAccess/Repositories/`

**UserRepository.cs** - Extends BaseRepository<User>
```csharp
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AuthDbContext context) : base(context) { }
    
    public async Task<User?> GetByEmailAsync(string email)
    public async Task<User?> GetUserWithRolesAsync(Guid userId)
    public async Task<User?> GetUserWithPermissionsAsync(Guid userId)
    public async Task<IEnumerable<User>> SearchUsersAsync(UserSearchDTO search)
}
```

**RoleRepository.cs** - Extends BaseRepository<Role>
```csharp
public class RoleRepository : BaseRepository<Role>
{
    public async Task<Role?> GetByNameAsync(string name)
    public async Task<Role?> GetRoleWithPermissionsAsync(Guid roleId)
}
```

**PermissionRepository.cs** - Extends BaseRepository<Permission>
```csharp
public class PermissionRepository : BaseRepository<Permission>
{
    public async Task<Permission?> GetByResourceActionAsync(string resource, string action)
}
```

**RefreshTokenRepository.cs** - Extends BaseRepository<RefreshToken>
```csharp
public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenValueAsync(string tokenValue)
    public async Task<IEnumerable<RefreshToken>> GetTokensByFamilyAsync(string family)
    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId)
}
```

### 9.3 Unit of Work
Create: `Inventorization.Auth.Domain/DataAccess/UnitOfWork/AuthUnitOfWork.cs`
```csharp
public class AuthUnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _context;
    
    public IUserRepository Users { get; }
    public IRepository<Role> Roles { get; }
    public IRepository<Permission> Permissions { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    public async Task BeginTransactionAsync()
    public async Task CommitTransactionAsync()
    public async Task RollbackTransactionAsync()
}
```

---

## Phase 10: Mappers, Creators, Modifiers, Validators

### 10.1 Mappers - ✅ COMPLETE
Create in: `Inventorization.Auth.Domain/Mappers/`

**UserMapper.cs** - Implements IMapper<User, UserDetailsDTO>
```csharp
public class UserMapper : IMapper<User, UserDetailsDTO>
{
    public UserDetailsDTO Map(User entity)
    public Expression<Func<User, UserDetailsDTO>> GetProjection()
}
```

**RoleMapper.cs** - Maps Role with nested PermissionInfoDTO collection
**PermissionMapper.cs** - Simple permission mapping with resource/action fields
- All use LINQ expressions for database projection (no N+1 queries)

### 10.2 Entity Creators - ✅ COMPLETE
Create in: `Inventorization.Auth.Domain/Creators/`

**UserCreator.cs** - Implements IEntityCreator<User, CreateUserDTO>
```csharp
public class UserCreator : IEntityCreator<User, CreateUserDTO>
{
    private readonly IPasswordHasher _passwordHasher;
    
    public UserCreator(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }
    
    public User Create(CreateUserDTO dto)
    {
        var hashedPassword = _passwordHasher.HashPassword(dto.Password);
        return new User(email: dto.Email, passwordHash: hashedPassword, fullName: dto.FullName);
    }
}
```

**RoleCreator.cs** - Creates Role from CreateRoleDTO
**PermissionCreator.cs** - Creates Permission with resource.action validation
- All use constructor dependency injection for IPasswordHasher (UserCreator only)

### 10.3 Entity Modifiers - ✅ COMPLETE
Create in: `Inventorization.Auth.Domain/Modifiers/`

**UserModifier.cs** - Implements IEntityModifier<User, UpdateUserDTO>
```csharp
public class UserModifier : IEntityModifier<User, UpdateUserDTO>
{
    private readonly IPasswordHasher _passwordHasher;
    
    public UserModifier(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }
    
    public void Modify(User entity, UpdateUserDTO dto)
    {
        entity.UpdateProfile(dto.Email, dto.FullName);
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            var hashedPassword = _passwordHasher.HashPassword(dto.NewPassword);
            entity.SetPassword(hashedPassword);
        }
        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value) entity.Activate();
            else entity.Deactivate();
        }
    }
}
```

**RoleModifier.cs** - Modifies Role name and description
**PermissionModifier.cs** - Modifies Permission name/resource/action
- All follow immutability pattern with dedicated update methods

### 10.4 Search Query Providers - ✅ COMPLETE
Create in: `Inventorization.Auth.Domain/SearchProviders/`

**UserSearchProvider.cs** - Implements ISearchQueryProvider<User, UserSearchDTO>
```csharp
public class UserSearchProvider : ISearchQueryProvider<User, UserSearchDTO>
{
    public Expression<Func<User, bool>> GetSearchExpression(UserSearchDTO searchDto)
    {
        return u =>
            (string.IsNullOrEmpty(searchDto.Email) || u.Email.Contains(searchDto.Email)) &&
            (string.IsNullOrEmpty(searchDto.FullName) || u.FullName.Contains(searchDto.FullName)) &&
            u.IsActive == searchDto.IsActive;
    }
}
```

**RoleSearchProvider.cs** - Filters by Name and Description (contains-based)
**PermissionSearchProvider.cs** - Filters by Name, Resource, Action
- All use Expression<Func<T, bool>> for server-side filtering

### 10.5 Validators - ✅ COMPLETE
Create in: `Inventorization.Auth.Domain/Validators/`

**CreateUserDtoValidator.cs** - Implements IValidator<CreateUserDTO>
```csharp
public class CreateUserDtoValidator : IValidator<CreateUserDTO>
{
    private readonly IUserRepository _userRepository;
    
    public CreateUserDtoValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    
    public async Task<ValidationResult> ValidateAsync(CreateUserDTO dto, CancellationToken cancellationToken = default)
    {
        // Email format check
        if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return ValidationResult.WithErrors("Invalid email format");
        
        // Email uniqueness check
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return ValidationResult.WithErrors("Email already in use");
        
        // Password length check
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            return ValidationResult.WithErrors("Password must be at least 8 characters");
        
        // Full name length check
        if (string.IsNullOrWhiteSpace(dto.FullName) || dto.FullName.Length < 2)
            return ValidationResult.WithErrors("Full name must be at least 2 characters");
        
        return ValidationResult.Ok();
    }
}
```

**UpdateUserDtoValidator.cs** - Validates email format, fullName, optional newPassword (8+ chars if provided)
**CreateRoleDtoValidator.cs** - Validates name (2+ chars)
**UpdateRoleDtoValidator.cs** - Same as create
**CreatePermissionDtoValidator.cs** - Validates name, resource, action (all 2+ chars)
**UpdatePermissionDtoValidator.cs** - Same as create
- All include null safety checks and appropriate error messages

### 10.6 New DTOs - ✅ COMPLETE
Added to: `Inventorization.Auth.DTO/DTO/`

**CreatePermissionDTO.cs** - Name, Resource, Action, Description
**UpdatePermissionDTO.cs** - Same fields (extends UpdateDTO base)
**DeletePermissionDTO.cs** - Empty (extends DeleteDTO base)

**UpdateUserDTO Enhanced** - Added NewPassword field for optional password updates
**RoleSearchDTO Enhanced** - Added Description field for filtering

---

## Phase 11: Data Services - ✅ COMPLETE

### 11.1 UserDataService - ✅ COMPLETE
Create: `Inventorization.Auth.Domain/DataServices/UserDataService.cs`

**Key Implementation Details:**
- Constructor injects: IAuthUnitOfWork, IMapper<User, UserDetailsDTO>, IEntityCreator, IEntityModifier, validators, ISearchQueryProvider, ILogger
- **NO IServiceProvider dependency** - All password hashing handled via injected IPasswordHasher in UserCreator/UserModifier
- GetByIdAsync: Retrieves user by ID, returns ServiceResult<UserDetailsDTO>
- AddAsync: Validates DTO, creates entity via creator, persists to database
- UpdateAsync: Validates DTO, modifies entity via modifier, persists changes
- DeleteAsync: Soft or hard delete based on configuration
- SearchAsync: Filters using search provider expressions, returns paginated PagedResult<UserDetailsDTO>

**Error Handling:**
- All operations wrapped in try-catch with logging
- Validation failures return ServiceResult with error messages
- Database failures logged and returned as ServiceResult failures

**Validation:**
- CreateUserDTO: Email format, email uniqueness, password length, full name length
- UpdateUserDTO: Email format, fullName, optional newPassword validation

### 11.2 RoleDataService - ✅ COMPLETE
Create: `Inventorization.Auth.Domain/DataServices/RoleDataService.cs`

- Identical pattern to UserDataService
- Manages Role entities with full CRUD
- Search filters by Name and Description
- Validates role name (2+ characters)

### 11.3 PermissionDataService - ✅ COMPLETE
Create: `Inventorization.Auth.Domain/DataServices/PermissionDataService.cs`

- Identical pattern to UserDataService
- Manages Permission entities with full CRUD
- Search filters by Name, Resource, Action
- Validates all fields (2+ characters each)

**Summary:**
✅ All 3 Data Services fully implemented and tested
✅ All dependencies through constructor injection
✅ All use IMapper for LINQ projection
✅ All use validators for input validation
✅ All return ServiceResult<T> for consistency
✅ Build: 0 Errors, 10 Warnings (acceptable)
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper<User, UserDetailsDTO> _mapper;
    private readonly IEntityCreator<User, CreateUserDTO> _creator;
    private readonly IEntityModifier<User, UpdateUserDTO> _modifier;
    private readonly IValidator<CreateUserDTO> _createValidator;
    private readonly IValidator<UpdateUserDTO> _updateValidator;
    private readonly ISearchQueryProvider<User, UserSearchDTO> _searchProvider;
    private readonly ILogger<UserDataService> _logger;

    public async Task<ServiceResult<UserDetailsDTO>> GetByIdAsync(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return ServiceResult<UserDetailsDTO>.NotFound("User not found");
            
            return ServiceResult<UserDetailsDTO>.Success(_mapper.Map(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {userId}", id);
            return ServiceResult<UserDetailsDTO>.Error("Failed to get user");
        }
    }

    public async Task<ServiceResult<UserDetailsDTO>> AddAsync(CreateUserDTO dto)
    {
        // Validate
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ServiceResult<UserDetailsDTO>.Failed(validationResult.Errors);

        try
        {
            // Create entity
            var user = _creator.Create(dto, _serviceProvider);
            
            // Add to repository
            await _userRepository.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return ServiceResult<UserDetailsDTO>.Success(_mapper.Map(user), "User created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return ServiceResult<UserDetailsDTO>.Error("Failed to create user");
        }
    }

    public async Task<ServiceResult<UserDetailsDTO>> UpdateAsync(Guid id, UpdateUserDTO dto)
    {
        // Similar validation and update logic
    }

    public async Task<ServiceResult<UserDetailsDTO>> DeleteAsync(Guid id, DeleteUserDTO dto)
    {
        // Similar delete logic
    }

    public async Task<ServiceResult<PageDTO<UserDetailsDTO>>> SearchAsync(UserSearchDTO search)
    {
        try
        {
            var filter = _searchProvider.GetFilter(search);
            var query = _userRepository.GetQueryable().Where(filter);
            
            var total = await query.CountAsync();
            var items = await query
                .Skip(search.Page.Skip)
                .Take(search.Page.Take)
                .Select(_mapper.MapExpression())
                .ToListAsync();
            
            return ServiceResult<PageDTO<UserDetailsDTO>>.Success(
                new PageDTO<UserDetailsDTO>
                {
                    Items = items,
                    TotalCount = total,
                    PageNumber = search.Page.PageNumber,
                    PageSize = search.Page.PageSize
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return ServiceResult<PageDTO<UserDetailsDTO>>.Error("Failed to search users");
        }
    }
}
```

**RoleDataService.cs** (IDataService<Role, ...>)
**PermissionDataService.cs** (IDataService<Permission, ...>)

### 11.2 Authentication-Specific Services
Keep existing services but adjust to support DataServices:

- **PasswordHasher** - Injected into UserCreator/UserModifier
- **JwtTokenProvider** - Used by AuthenticationService
- **TokenRotationService** - Handles refresh token logic
- **AuthorizationService** - Checks user permissions

---

## Phase 12: API Layer

### 12.1 API Base Controllers (Inventorization.Auth.API.Base)
Create: `Inventorization.Auth.API.Base/Controllers/`

**DataController<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO, TService>**
- GET /{id}
- POST /
- PUT /{id}
- DELETE /{id}

**SearchController<TEntity, TDetailsDTO, TSearchDTO, TService>**
- POST /search

### 12.2 Concrete Controllers (Inventorization.Auth.API)
Create: `Inventorization.Auth.API/Controllers/`

**UsersController** - Extends DataController<User, ...>
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : DataController<User, CreateUserDTO, UpdateUserDTO, DeleteUserDTO, UserDetailsDTO, UserSearchDTO, IUserDataService>
{
    public UsersController(IUserDataService userService, ILogger<UsersController> logger) 
        : base(userService, logger) { }
}
```

**RolesController** - Extends DataController<Role, ...>
**PermissionsController** - Extends DataController<Permission, ...>

**AuthController** - Custom controller (not following DataController pattern)
```csharp
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestDTO request)
    
    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
}
```

---

## Phase 13: Supporting Infrastructure

### 13.1 Database Seeding
Create: `Inventorization.Auth.Domain/Seeding/AuthDbSeeder.cs`
- Seed default roles (Admin, Manager, Viewer, User)
- Seed default permissions (product.create, product.read, etc.)
- Seed admin user for testing

### 13.2 Program.cs Configuration
Register all services:
- Repositories
- UnitOfWork
- Mappers
- Creators/Modifiers
- Validators
- DataServices
- PasswordHasher, JwtTokenProvider, etc.

### 13.3 Docker & Configuration
- Update docker-compose.yml with auth-service
- Configure appsettings.json with JWT, database, etc.

---

## Phase 14: gRPC Inter-Service Communication

### 14.1 gRPC Proto Definition
Create: `Inventorization.Auth.gRPC/Protos/permission-service.proto`
```proto
service PermissionService {
    rpc CheckPermission(CheckPermissionRequest) returns (PermissionResponse);
    rpc CheckPermissions(CheckPermissionsRequest) returns (PermissionResponse);
}
```

### 14.2 gRPC Server Implementation
Create: `Inventorization.Auth.gRPC/Services/PermissionGrpcService.cs`
- Implements CheckPermission, CheckPermissions methods
- Uses IRolePermissionService for authorization checks

### 14.3 gRPC Clients (Other Services)
Add to each microservice that needs permission checks:
- IPermissionClient interface in Inventorization.Base
- GrpcPermissionClient implementation with retry policies

---

## Summary

| Phase | Deliverables | Projects |
|-------|--------------|----------|
| 9 | Repositories, UnitOfWork | Auth.Domain |
| 10 | Mappers, Creators, Modifiers, Validators, SearchProviders | Auth.Domain |
| 11 | IDataService implementations (User, Role, Permission) | Auth.Domain |
| 12 | API Controllers, AuthController | Auth.API, Auth.API.Base |
| 13 | Database Seeding, Program.cs Configuration, Docker | Auth.Domain, Auth.API |
| 14 | gRPC Proto, PermissionGrpcService, gRPC Clients | Auth.gRPC, Base, All Services |

**Total Entities with Full CRUD Support:** 3 (User, Role, Permission)
**Custom Authentication Endpoints:** 3 (login, refresh, logout)
**Inter-Service Communication:** gRPC PermissionService
