# Backend Architecture Quick Reference

## Key Abstractions from `Inventorization.Base`

### DTOs
```csharp
// Base classes to inherit from
public abstract class CreateDTO { }
public abstract class UpdateDTO { Guid Id { get; set; } }
public abstract class DetailsDTO : BaseDTO { }
public abstract class SearchDTO { PageDTO? Page { get; set; } }

// Usage example
public class CreateProductDTO : CreateDTO
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class UpdateProductDTO : UpdateDTO
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class ProductDetailsDTO : DetailsDTO
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Service Mappers
```csharp
// Create a mapper implementing IMapper<TEntity, TDetailsDTO>
public class ProductMapper : IMapper<Product, ProductDetailsDTO>
{
    public ProductDetailsDTO Map(Product entity)
    {
        return new ProductDetailsDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price,
            CreatedAt = entity.CreatedAt
        };
    }

    public Expression<Func<Product, ProductDetailsDTO>> GetProjection()
    {
        return p => new ProductDetailsDTO
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CreatedAt = p.CreatedAt
        };
    }
}
```

### Entity Creation
```csharp
// Create an entity creator implementing IEntityCreator<TEntity, TCreateDTO>
public class ProductCreator : IEntityCreator<Product, CreateProductDTO>
{
    public Product Create(CreateProductDTO dto)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Price = dto.Price,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### Entity Modification
```csharp
// Create an entity modifier implementing IEntityModifier<TEntity, TUpdateDTO>
public class ProductModifier : IEntityModifier<Product, UpdateProductDTO>
{
    public void Modify(Product entity, UpdateProductDTO dto)
    {
        entity.Name = dto.Name;
        entity.Price = dto.Price;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
```

### Search Queries
```csharp
// Create a search DTO
public class ProductSearchDTO : SearchDTO
{
    public string? NameFilter { get; set; }
    public decimal? MinPrice { get; set; }
}

// Create a search query provider implementing ISearchQueryProvider
public class ProductSearchProvider : ISearchQueryProvider<Product, ProductSearchDTO>
{
    public Expression<Func<Product, bool>> GetSearchExpression(ProductSearchDTO searchDto)
    {
        return p => 
            (string.IsNullOrEmpty(searchDto.NameFilter) || p.Name.Contains(searchDto.NameFilter)) &&
            (!searchDto.MinPrice.HasValue || p.Price >= searchDto.MinPrice.Value);
    }
}
```

### Service Implementation
```csharp
// Implement IDataService using injected abstractions
public class ProductService : IDataService<
    Product, 
    CreateProductDTO, 
    UpdateProductDTO, 
    DeleteDTO, 
    ProductDetailsDTO, 
    ProductSearchDTO>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper<Product, ProductDetailsDTO> _mapper;
    private readonly IEntityCreator<Product, CreateProductDTO> _creator;
    private readonly IEntityModifier<Product, UpdateProductDTO> _modifier;
    private readonly ISearchQueryProvider<Product, ProductSearchDTO> _searchProvider;
    private readonly IValidator<CreateProductDTO> _createValidator;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IRepository<Product> repository,
        IMapper<Product, ProductDetailsDTO> mapper,
        IEntityCreator<Product, CreateProductDTO> creator,
        IEntityModifier<Product, UpdateProductDTO> modifier,
        ISearchQueryProvider<Product, ProductSearchDTO> searchProvider,
        IValidator<CreateProductDTO> createValidator,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _creator = creator;
        _modifier = modifier;
        _searchProvider = searchProvider;
        _createValidator = createValidator;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<ProductDetailsDTO>> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ServiceResult<ProductDetailsDTO>.Failure("Product not found");

            return ServiceResult<ProductDetailsDTO>.Success(_mapper.Map(entity));
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDetailsDTO>.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult<ProductDetailsDTO>> AddAsync(
        CreateProductDTO createDto, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = await _createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validation.IsValid)
                return ServiceResult<ProductDetailsDTO>.Failure(validation.Errors);

            var entity = _creator.Create(createDto);
            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<ProductDetailsDTO>.Success(_mapper.Map(entity));
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDetailsDTO>.Failure(ex.Message);
        }
    }

    // ... other CRUD methods
}
```

### Dependency Injection Registration
```csharp
// In Program.cs
services
    // Mappers
    .AddScoped<IMapper<Product, ProductDetailsDTO>, ProductMapper>()
    
    // Entity operations
    .AddScoped<IEntityCreator<Product, CreateProductDTO>, ProductCreator>()
    .AddScoped<IEntityModifier<Product, UpdateProductDTO>, ProductModifier>()
    
    // Search
    .AddScoped<ISearchQueryProvider<Product, ProductSearchDTO>, ProductSearchProvider>()
    
    // Validators
    .AddScoped<IValidator<CreateProductDTO>, CreateProductValidator>()
    
    // Services (ALWAYS as interface, never concrete type)
    .AddScoped<IDataService<Product, CreateProductDTO, UpdateProductDTO, DeleteDTO, 
        ProductDetailsDTO, ProductSearchDTO>, ProductService>()
    
    // Unit of Work
    .AddScoped<IUnitOfWork, EFUnitOfWork>();
```

## Important Rules

✅ **Always inject as interfaces** - Never inject concrete types
✅ **Use Inventorization.Base abstractions** - All base classes and interfaces come from this project
✅ **DTOs in subfolders** - Organize DTOs in `/DTO/[EntityName]/` folders
✅ **Immutable DTOs in responses** - Use init-only properties
✅ **Consistent naming** - Follow `[Entity][Operation]DTO` pattern
✅ **Test everything** - Every service must have unit tests
✅ **Use ServiceResult** - Always return `ServiceResult<T>` from services
✅ **Handle errors properly** - Catch exceptions and return failures

## Common Patterns

### Adding a New Entity
1. Create DTOs in `InventorySystem.DTOs/DTO/[EntityName]/`
2. Create Entity in Domain project
3. Create Mapper implementing `IMapper<TEntity, TDetailsDTO>`
4. Create Creator, Modifier, SearchProvider if needed
5. Create Service implementing `IDataService<...>`
6. Create Tests covering all operations
7. Register in DI container
8. Create Controller using the Service

### Testing a Service
```csharp
[TestClass]
public class ProductServiceTests
{
    private Mock<IRepository<Product>> _repositoryMock;
    private Mock<IMapper<Product, ProductDetailsDTO>> _mapperMock;
    private ProductService _service;

    [TestInitialize]
    public void Setup()
    {
        _repositoryMock = new Mock<IRepository<Product>>();
        _mapperMock = new Mock<IMapper<Product, ProductDetailsDTO>>();
        _service = new ProductService(_repositoryMock.Object, _mapperMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Test" };
        var dto = new ProductDetailsDTO { Id = id, Name = "Test" };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map(product))
            .Returns(dto);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(dto.Id, result.Data.Id);
    }
}
```

## See Also
- [Architecture.md](Architecture.md) - Complete architecture specifications
- [ARCHITECTURE_REFACTORING.md](ARCHITECTURE_REFACTORING.md) - Refactoring summary
- [copilot-instructions.md](.github/copilot-instructions.md) - Development guidelines
