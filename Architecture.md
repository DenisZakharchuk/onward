# Inventorization dashboard
 Project consist of multiple layers:
  - Frontend: vue 3 application
  - Backend: multiple .net8 asp.net microservices, they use entity framework to interract with sql db - postgresql
  - DB: postgresql, containerized (docker), MongoDB server
  - message broker

# Frontend
 It's regular dashboard application. 
 It has authorization user story. For now it has local authorization (using AuthoService microservice).
 It has set of list views and form views for entities of the DataModel;
 Business loic layer is implemented in separate abstractions in "services" folder. They are injected where they needed with provide-use pattern
 
# Backend
 Microservice arhiteture. Each microservice has 
 - separate, dedicated DB (if it's needed). 
 - restfull API implemented in controllers
 - API can require authorization (using bearer JWT auth method), or allow anonymous authorization (for example for AuthoService)
 - swagger
 - business logic layer implemented in separate project
 We need to have separate microservice (with dedicated Data Storage) for each set of interconnected entities
 Naming conventions for projects: 
 - DTO projects: Inventorization.[BoundedContextName].DTO; class library;
 - Inventorization.[BoundedContextName].Domain; class library; It has folders: Entities, Services, DbContexts, UOWs. Inventorization.[BoundedContextName].DTO is a dependency;
 - Inventorization.[BoundedContextName].API; asp.net web app; Inventorization.[BoundedContextName].Domain and Inventorization.[BoundedContextName].DTO are dependencies
 all microservices are containarized;
 maintein docker-compose file in workspace root directory for all microservices

# Backend Architecture Rules & Patterns
## Dependency Injection Rules
- All DataService implementations (e.g., CustomerService) must be injected as their corresponding IDataService<T...> or specific interface (e.g., ICustomerService), not as concrete types.
- All service, repository, and abstraction dependencies should be injected as interfaces, not concrete types, to maximize testability and flexibility.
## Testing Rules
- Every API project must have a corresponding unittest project (e.g., Inventorization.[BoundedContextName].API.Tests).
- Every concrete abstraction (service, repository, creator, modifier, query provider, etc.) must be covered with unit tests in the unittest project.

## General Principles
  - DTO projects: `Inventorization.[BoundedContextName].DTO` (class library)
  - Domain projects: `Inventorization.[BoundedContextName].Domain` (class library, with Entities, Services, DbContexts, UOWs)
  - API projects: `Inventorization.[BoundedContextName].API` (ASP.NET web app)

## Base Abstractions and Data Structures
- All base abstractions and common data structures (such as `CreateDTO`, `UpdateDTO`, `DeleteDTO`, `DetailsDTO`, `SearchDTO`, `PageDTO`, `ServiceResult<T>`, and all generic interfaces like `IEntityCreator`, `IEntityModifier`, `ISearchQueryProvider`, `IMapper`, etc.) must be located in a separate shared project named `Inventorization.Base`.
- All bounded context/domain projects must reference `Inventorization.Base` for these shared types.
- All DTOs for each bounded context must be placed in the corresponding DTO project under a `DTO/` subfolder (e.g., `DTO/Customer`).
- All mapping logic (entity-to-DTO and DTO-to-entity) must use the `IMapper<TEntity, TDetailsDTO>` abstraction, supporting both object mapping and LINQ projection via `Expression<Func<TEntity, TDetailsDTO>>`.

## Domain Service Abstractions
Each entity has a corresponding set of DTOs:
  - `DetailsDTO`: returned by `GetByIdAsync`
  - `CreateDTO`: input for `AddAsync`
  - `UpdateDTO`: input for `UpdateAsync`
  - `DeleteDTO`: input for `DeleteAsync`
  - `SearchDTO`: input for `SearchAsync`
All base DTOs should be defined in `Inventorization.Base` and inherited/extended in each bounded context as needed. All usages must reference the DTOs from the DTO project, not from Domain.
Each concrete `DataService` is generic over all relevant DTOs and must use the `IMapper` abstraction for mapping and projection.
All DataService implementations must be injected as their interface (e.g., `ICustomerService` or `IDataService<T...>`), never as a concrete type.

### DTO Typing Rules
- Each entity has a corresponding set of DTOs, all located in the DTO project:
  - `DetailsDTO`: returned by `GetByIdAsync`
  - `CreateDTO`: input for `AddAsync`
  - `UpdateDTO`: input for `UpdateAsync`
  - `DeleteDTO`: input for `DeleteAsync`
  - `SearchDTO`: input for `SearchAsync`
- Each concrete `DataService` is generic over all relevant DTOs and must use the `IMapper` abstraction for mapping and projection.

### Search Abstraction
- `SearchDTO` base class includes:
  - `PageDTO` for pagination
  - Abstract generic properties: `FilterDTO`, `ProjectionDTO` (implemented in each concrete `SearchDTO`)
  - Optionally, add `SortDTO` for sorting

### Entity Mapping Abstractions
- Use the following abstractions:
  - `IMapper<TEntity, TDetailsDTO>`: provides both object mapping and LINQ projection for entity-to-DTO mapping; injected and used in all DataService methods.
  - `IEntityCreator<TEntity, TCreateDTO>`: creates entity from `CreateDTO`, injected and used in `AddAsync`.
  - `IEntityModifier<TEntity, TUpdateDTO>`: updates entity from `UpdateDTO`, injected and used in `UpdateAsync`.
  - `ISearchQueryProvider<TEntity, TSearchDTO>`: creates LINQ expression for search, injected and used in `SearchAsync`.
  - Optionally, `IDetailsMapper<TEntity, TDetailsDTO>` and `IProjectionMapper<TEntity, TProjectionDTO>` for advanced mapping scenarios.

### Validation
- Inject validators (e.g., `IValidator<TCreateDTO>`, `IValidator<TUpdateDTO>`) for input validation before entity creation/modification.

### Authorization
- Optionally inject `IAuthorizationService` to check permissions in service methods.

### Domain Events
- Allow `DataService` to publish domain events (e.g., via `IDomainEventPublisher`) after significant changes.

### Caching
- Optionally, add a caching decorator for read-heavy services.

### Soft Delete
- If soft delete is supported, ensure repository/service methods respect this.

### Unit of Work
- All changes are committed atomically via injected `IUnitOfWork`.

### Error Handling & Results
- Use a base `ServiceResult<T>` type for consistent result/wrap error handling.

### Observability
- Add logging, metrics, and tracing as needed in all service methods.

### CQRS (Advanced)
- For complex domains, consider separating read/write services (CQRS).

### Documentation
- Document all abstractions and expected extension points in code and markdown.

---

## Controller Architecture Patterns

All API controllers must follow an abstract generic base class pattern to eliminate code duplication and maintain consistency. See [CONTROLLER_ARCHITECTURE.md](CONTROLLER_ARCHITECTURE.md) for complete guidance.

### Project Structure
- **Inventorization.[BoundedContextName].API.Base** - Shared abstract controllers (non-generic and generic)
  - `Controllers/ServiceController.cs` - Non-generic base extending `ControllerBase`
  - `Controllers/DataController.cs` - Generic CRUD base extending `ServiceController`
  - `Controllers/SearchController.cs` - Generic search base extending `ServiceController`
- **Inventorization.[BoundedContextName].API** - Concrete controllers
  - References `...API.Base` project
  - All controllers inherit from generic base classes
  - Folder structure mirrors base controller types:
    - `Controllers/Data/` - Concrete controllers extending `DataController<...>`
    - `Controllers/Search/` - Concrete controllers extending `SearchController<...>`
    - `Controllers/[CustomType]/` - Other specialized controllers as needed

### ServiceController
Abstract non-generic base class providing common controller functionality:
```csharp
public abstract class ServiceController : ControllerBase
{
    protected readonly ILogger<ServiceController> Logger;
    
    protected ServiceController(ILogger<ServiceController> logger)
    {
        Logger = logger;
    }
}
```

### DataController<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO, TService>
Abstract generic base class extending `ServiceController` providing all CRUD operations:
- `GET /{id}` → `GetByIdAsync(id)` - Returns `ServiceResult<TDetailsDTO>`
- `POST /` → `CreateAsync(dto)` - Returns `ServiceResult<TDetailsDTO>` with 201 Created
- `PUT /{id}` → `UpdateAsync(id, dto)` - Returns `ServiceResult<TDetailsDTO>`
- `DELETE /{id}` → `DeleteAsync(id, dto)` - Returns `ServiceResult<TDetailsDTO>`

Generic constraints:
- `TService : IDataService<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>`
- All DTOs must inherit from corresponding base DTOs in `Inventorization.Base`

Concrete controllers inherit from `DataController<...>` and require only:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : DataController<Product, CreateProductDTO, UpdateProductDTO, DeleteProductDTO, ProductDetailsDTO, ProductSearchDTO, IProductService>
{
    public ProductsController(IProductService service) : base(service) { }
}
```

### SearchController<TEntity, TDetailsDTO, TSearchDTO, TService>
Abstract base class for complex search, filtering, and pagination:
- `POST /search` → `SearchAsync(dto)` - Returns paginated results with `ServiceResult<PageDTO<TDetailsDTO>>`
- Supports custom filters, projections, and sorting via `SearchDTO`

Used when search complexity exceeds simple `GetByIdAsync` queries. Inherited alongside or instead of `DataController` depending on entity requirements.

### Controller Design Principles
1. **DRY (Don't Repeat Yourself)**: Eliminate repeated HTTP verb handling and response mapping
2. **Open/Closed Principle**: New controllers extend base classes without modifying existing ones
3. **Template Method Pattern**: Base class defines structure, concrete controllers specify types
4. **Type Safety**: All 7 generic type parameters enforced at compile-time
5. **Minimal Concrete Code**: Concrete controllers should be 3-5 lines (declaration + constructor only)

### HTTP Status Code Mapping
- `200 OK`: `GetByIdAsync`, `UpdateAsync`, `SearchAsync` success
- `201 Created`: `CreateAsync` success
- `400 Bad Request`: DTO validation failure or domain logic error
- `404 Not Found`: Entity not found during Get/Update/Delete
- `500 Internal Server Error`: Unexpected exceptions

### Adding New Entity Controllers
1. Verify entity has corresponding DTOs in the DTO project (DetailsDTO, CreateDTO, UpdateDTO, DeleteDTO, SearchDTO)
2. Verify `IDataService<...>` interface exists for the entity
3. Register service in `Program.cs` dependency injection
4. Create concrete controller class inheriting from `DataController<...>` or `SearchController<...>`
5. Define only the route attribute and constructor; inherit all HTTP methods
6. Add controller tests covering inherited methods with mocked service
7. Update Swagger documentation if custom attributes needed

See [CONTROLLER_ARCHITECTURE.md](CONTROLLER_ARCHITECTURE.md) for complete implementation guide, examples, and testing strategies.

---

## Additional Rules & Conventions
- All usages of DTOs, mapping, and projection logic must reference the DTO project and use the `IMapper` abstraction.
- All dependency injection must be by interface, not concrete type.
- All API projects must have a corresponding unit test project covering all concrete abstractions (service, repository, creator, modifier, query provider, etc.).
- Swagger UI must be enabled and served at the root URL in Development mode for all API projects.
- All build, bin, obj, and IDE-specific files must be excluded from version control via `.gitignore`.

**All further backend code, services, and patterns must follow these rules and abstractions.**

# DB
 basically docker-compose file with set of docker images based on official postgresql; each have persistent volumes;

# other
 message broker and MongoDB - also in containers

# DataModel
 described in a separate file DataModel.md