using InventorySystem.API.GraphQL;
using InventorySystem.AuditLog.Abstractions;
using InventorySystem.AuditLog.Services;
using InventorySystem.Business.Abstractions;
using InventorySystem.Business.Abstractions.Services;
using InventorySystem.Business.Services;
using InventorySystem.Business.DataServices;
using InventorySystem.Business.Mappers;
using InventorySystem.Business.Creators;
using InventorySystem.Business.Modifiers;
using InventorySystem.Business.SearchProviders;
using InventorySystem.Business.Validators;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Repositories;
using InventorySystem.DataAccess.Models;
using InventorySystem.DTOs.DTO.Product;
using InventorySystem.DTOs.DTO.Category;
using InventorySystem.DTOs.DTO.StockMovement;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DTOs;
using MongoDB.Driver;
using IUnitOfWork = InventorySystem.DataAccess.Abstractions.IUnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repositories as singletons (in-memory implementation)
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();
builder.Services.AddSingleton<IStockMovementRepository, InMemoryStockMovementRepository>();
builder.Services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();

// Configure MongoDB
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://admin:admin123@localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "inventorydb";

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton<IAuditRepository>(sp =>
    new MongoAuditRepository(sp.GetRequiredService<IMongoClient>(), mongoDatabaseName));

// Configure Audit Logging
builder.Services.AddSingleton<IAuditLogger, MongoAuditLogger>();

// Register data services (new generic services using IDataService pattern)
builder.Services.AddScoped<IProductService, ProductDataService>();
builder.Services.AddScoped<ICategoryService, CategoryDataService>();
builder.Services.AddScoped<IStockMovementService, StockMovementDataService>();

// Register Product abstractions
builder.Services.AddScoped<IMapper<Product, ProductDetailsDTO>, ProductMapper>();
builder.Services.AddScoped<IEntityCreator<Product, CreateProductDTO>, ProductCreator>();
builder.Services.AddScoped<IEntityModifier<Product, UpdateProductDTO>, ProductModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Product, ProductSearchDTO>, ProductSearchProvider>();
builder.Services.AddScoped<IValidator<CreateProductDTO>, CreateProductValidator>();
builder.Services.AddScoped<IValidator<UpdateProductDTO>, UpdateProductValidator>();

// Register Category abstractions
builder.Services.AddScoped<IMapper<Category, CategoryDetailsDTO>, CategoryMapper>();
builder.Services.AddScoped<IEntityCreator<Category, CreateCategoryDTO>, CategoryCreator>();
builder.Services.AddScoped<IEntityModifier<Category, UpdateCategoryDTO>, CategoryModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Category, CategorySearchDTO>, CategorySearchProvider>();
builder.Services.AddScoped<IValidator<CreateCategoryDTO>, CreateCategoryValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryDTO>, UpdateCategoryValidator>();

// Register StockMovement abstractions
builder.Services.AddScoped<IMapper<StockMovement, StockMovementDetailsDTO>, StockMovementMapper>();
builder.Services.AddScoped<IEntityCreator<StockMovement, CreateStockMovementDTO>, StockMovementCreator>();
builder.Services.AddScoped<ISearchQueryProvider<StockMovement, StockMovementSearchDTO>, StockMovementSearchProvider>();
builder.Services.AddScoped<IValidator<CreateStockMovementDTO>, CreateStockMovementValidator>();

// Register legacy business services (for backward compatibility)
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<StockService>();

// Add HTTP Context Accessor for audit logging
builder.Services.AddHttpContextAccessor();

// Configure GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<AuditLogQuery>()
    .AddMongoDbProjections();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGraphQL("/graphql");

app.Run();
