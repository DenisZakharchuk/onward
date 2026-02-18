using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// ===== Database Configuration =====
var connectionString = builder.Configuration.GetConnectionString("GoodsDatabase") 
    ?? throw new InvalidOperationException("Connection string 'GoodsDatabase' not found.");

builder.Services.AddDbContext<Inventorization.Goods.BL.DbContexts.GoodsDbContext>(options =>
    options.UseNpgsql(connectionString));

// ===== JWT Authentication Configuration =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? "Inventorization.Auth";
var audience = jwtSettings["Audience"] ?? "Inventorization.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ===== Repository & UnitOfWork Registration =====
// Register DbContext as both specific type and base DbContext for repositories
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<Inventorization.Goods.BL.DbContexts.GoodsDbContext>());

// Generic repository registration
builder.Services.AddScoped(typeof(IRepository<>), typeof(Inventorization.Base.DataAccess.BaseRepository<>));

// UnitOfWork registration
builder.Services.AddScoped<Inventorization.Goods.BL.DataAccess.IGoodsUnitOfWork, Inventorization.Goods.BL.DataAccess.GoodsUnitOfWork>();
builder.Services.AddScoped<Inventorization.Base.DataAccess.IUnitOfWork>(sp => sp.GetRequiredService<Inventorization.Goods.BL.DataAccess.IGoodsUnitOfWork>());

// ===== Mappers, Creators, Modifiers, SearchProviders, Validators =====
// Good entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.BL.Entities.Good, Inventorization.Goods.DTO.DTO.Good.GoodDetailsDTO>, 
    Inventorization.Goods.BL.Mappers.GoodMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.BL.Entities.Good, Inventorization.Goods.DTO.DTO.Good.CreateGoodDTO>, 
    Inventorization.Goods.BL.Creators.GoodCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.BL.Entities.Good, Inventorization.Goods.DTO.DTO.Good.UpdateGoodDTO>, 
    Inventorization.Goods.BL.Modifiers.GoodModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.BL.Entities.Good, Inventorization.Goods.DTO.DTO.Good.GoodSearchDTO>, 
    Inventorization.Goods.BL.SearchProviders.GoodSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Good.CreateGoodDTO>, 
    Inventorization.Goods.BL.Validators.CreateGoodValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Good.UpdateGoodDTO>, 
    Inventorization.Goods.BL.Validators.UpdateGoodValidator>();

// ===== ADT-Based Search Components for Good =====
// Query builder for ADT-based search
builder.Services.AddScoped<IQueryBuilder<Inventorization.Goods.BL.Entities.Good>, 
    Inventorization.Goods.BL.DataAccess.GoodQueryBuilder>();

// Projection mapper
builder.Services.AddScoped<Inventorization.Goods.BL.Mappers.Projection.IGoodProjectionMapper, 
    Inventorization.Goods.BL.Mappers.Projection.GoodProjectionMapper>();

// Projection expression builder for transformations
builder.Services.AddScoped<Inventorization.Base.Services.ProjectionExpressionBuilder>();

// Search query validator
builder.Services.AddScoped<IValidator<Inventorization.Base.ADTs.SearchQuery>, 
    Inventorization.Goods.BL.Validators.GoodSearchQueryValidator>();

// Search service (concrete type for dual method support)
builder.Services.AddScoped<Inventorization.Goods.BL.Services.GoodSearchService>();
builder.Services.AddScoped<ISearchService<Inventorization.Goods.BL.Entities.Good, Inventorization.Goods.DTO.ADTs.GoodProjection>>(
    sp => sp.GetRequiredService<Inventorization.Goods.BL.Services.GoodSearchService>());

// Category entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.BL.Entities.Category, Inventorization.Goods.DTO.DTO.Category.CategoryDetailsDTO>, 
    Inventorization.Goods.BL.Mappers.CategoryMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.BL.Entities.Category, Inventorization.Goods.DTO.DTO.Category.CreateCategoryDTO>, 
    Inventorization.Goods.BL.Creators.CategoryCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.BL.Entities.Category, Inventorization.Goods.DTO.DTO.Category.UpdateCategoryDTO>, 
    Inventorization.Goods.BL.Modifiers.CategoryModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.BL.Entities.Category, Inventorization.Goods.DTO.DTO.Category.CategorySearchDTO>, 
    Inventorization.Goods.BL.SearchProviders.CategorySearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Category.CreateCategoryDTO>, 
    Inventorization.Goods.BL.Validators.CreateCategoryValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Category.UpdateCategoryDTO>, 
    Inventorization.Goods.BL.Validators.UpdateCategoryValidator>();

// ===== ADT-Based Search Components for Category =====
// Query builder for ADT-based search
builder.Services.AddScoped<IQueryBuilder<Inventorization.Goods.BL.Entities.Category>, 
    Inventorization.Goods.BL.DataAccess.CategoryQueryBuilder>();

// Projection mapper
builder.Services.AddScoped<Inventorization.Goods.BL.Mappers.Projection.ICategoryProjectionMapper, 
    Inventorization.Goods.BL.Mappers.Projection.CategoryProjectionMapper>();

// Search query validator
builder.Services.AddScoped<Inventorization.Goods.BL.Validators.CategorySearchQueryValidator>();

// Search service (concrete type for dual method support)
builder.Services.AddScoped<Inventorization.Goods.BL.Services.CategorySearchService>();
builder.Services.AddScoped<ISearchService<Inventorization.Goods.BL.Entities.Category, Inventorization.Goods.DTO.ADTs.CategoryProjection>>(
    sp => sp.GetRequiredService<Inventorization.Goods.BL.Services.CategorySearchService>());

// Supplier entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.BL.Entities.Supplier, Inventorization.Goods.DTO.DTO.Supplier.SupplierDetailsDTO>, 
    Inventorization.Goods.BL.Mappers.SupplierMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.BL.Entities.Supplier, Inventorization.Goods.DTO.DTO.Supplier.CreateSupplierDTO>, 
    Inventorization.Goods.BL.Creators.SupplierCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.BL.Entities.Supplier, Inventorization.Goods.DTO.DTO.Supplier.UpdateSupplierDTO>, 
    Inventorization.Goods.BL.Modifiers.SupplierModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.BL.Entities.Supplier, Inventorization.Goods.DTO.DTO.Supplier.SupplierSearchDTO>, 
    Inventorization.Goods.BL.SearchProviders.SupplierSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Supplier.CreateSupplierDTO>, 
    Inventorization.Goods.BL.Validators.CreateSupplierValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Supplier.UpdateSupplierDTO>, 
    Inventorization.Goods.BL.Validators.UpdateSupplierValidator>();

// Warehouse entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.BL.Entities.Warehouse, Inventorization.Goods.DTO.DTO.Warehouse.WarehouseDetailsDTO>, 
    Inventorization.Goods.BL.Mappers.WarehouseMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.BL.Entities.Warehouse, Inventorization.Goods.DTO.DTO.Warehouse.CreateWarehouseDTO>, 
    Inventorization.Goods.BL.Creators.WarehouseCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.BL.Entities.Warehouse, Inventorization.Goods.DTO.DTO.Warehouse.UpdateWarehouseDTO>, 
    Inventorization.Goods.BL.Modifiers.WarehouseModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.BL.Entities.Warehouse, Inventorization.Goods.DTO.DTO.Warehouse.WarehouseSearchDTO>, 
    Inventorization.Goods.BL.SearchProviders.WarehouseSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Warehouse.CreateWarehouseDTO>, 
    Inventorization.Goods.BL.Validators.CreateWarehouseValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Warehouse.UpdateWarehouseDTO>, 
    Inventorization.Goods.BL.Validators.UpdateWarehouseValidator>();

// StockLocation entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.BL.Entities.StockLocation, Inventorization.Goods.DTO.DTO.StockLocation.StockLocationDetailsDTO>, 
    Inventorization.Goods.BL.Mappers.StockLocationMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.BL.Entities.StockLocation, Inventorization.Goods.DTO.DTO.StockLocation.CreateStockLocationDTO>, 
    Inventorization.Goods.BL.Creators.StockLocationCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.BL.Entities.StockLocation, Inventorization.Goods.DTO.DTO.StockLocation.UpdateStockLocationDTO>, 
    Inventorization.Goods.BL.Modifiers.StockLocationModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.BL.Entities.StockLocation, Inventorization.Goods.DTO.DTO.StockLocation.StockLocationSearchDTO>, 
    Inventorization.Goods.BL.SearchProviders.StockLocationSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.StockLocation.CreateStockLocationDTO>, 
    Inventorization.Goods.BL.Validators.CreateStockLocationValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.StockLocation.UpdateStockLocationDTO>, 
    Inventorization.Goods.BL.Validators.UpdateStockLocationValidator>();

// StockItem entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.BL.Entities.StockItem, Inventorization.Goods.DTO.DTO.StockItem.StockItemDetailsDTO>, 
    Inventorization.Goods.BL.Mappers.StockItemMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.BL.Entities.StockItem, Inventorization.Goods.DTO.DTO.StockItem.CreateStockItemDTO>, 
    Inventorization.Goods.BL.Creators.StockItemCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.BL.Entities.StockItem, Inventorization.Goods.DTO.DTO.StockItem.UpdateStockItemDTO>, 
    Inventorization.Goods.BL.Modifiers.StockItemModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.BL.Entities.StockItem, Inventorization.Goods.DTO.DTO.StockItem.StockItemSearchDTO>, 
    Inventorization.Goods.BL.SearchProviders.StockItemSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.StockItem.CreateStockItemDTO>, 
    Inventorization.Goods.BL.Validators.CreateStockItemValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.StockItem.UpdateStockItemDTO>, 
    Inventorization.Goods.BL.Validators.UpdateStockItemValidator>();

// PurchaseOrder entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.BL.Entities.PurchaseOrder, Inventorization.Goods.DTO.DTO.PurchaseOrder.PurchaseOrderDetailsDTO>, 
    Inventorization.Goods.BL.Mappers.PurchaseOrderMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.BL.Entities.PurchaseOrder, Inventorization.Goods.DTO.DTO.PurchaseOrder.CreatePurchaseOrderDTO>, 
    Inventorization.Goods.BL.Creators.PurchaseOrderCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.BL.Entities.PurchaseOrder, Inventorization.Goods.DTO.DTO.PurchaseOrder.UpdatePurchaseOrderDTO>, 
    Inventorization.Goods.BL.Modifiers.PurchaseOrderModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.BL.Entities.PurchaseOrder, Inventorization.Goods.DTO.DTO.PurchaseOrder.PurchaseOrderSearchDTO>, 
    Inventorization.Goods.BL.SearchProviders.PurchaseOrderSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.PurchaseOrder.CreatePurchaseOrderDTO>, 
    Inventorization.Goods.BL.Validators.CreatePurchaseOrderValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.PurchaseOrder.UpdatePurchaseOrderDTO>, 
    Inventorization.Goods.BL.Validators.UpdatePurchaseOrderValidator>();

// PurchaseOrderItem entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.BL.Entities.PurchaseOrderItem, Inventorization.Goods.DTO.DTO.PurchaseOrderItem.PurchaseOrderItemDetailsDTO>, 
    Inventorization.Goods.BL.Mappers.PurchaseOrderItemMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.BL.Entities.PurchaseOrderItem, Inventorization.Goods.DTO.DTO.PurchaseOrderItem.CreatePurchaseOrderItemDTO>, 
    Inventorization.Goods.BL.Creators.PurchaseOrderItemCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.BL.Entities.PurchaseOrderItem, Inventorization.Goods.DTO.DTO.PurchaseOrderItem.UpdatePurchaseOrderItemDTO>, 
    Inventorization.Goods.BL.Modifiers.PurchaseOrderItemModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.BL.Entities.PurchaseOrderItem, Inventorization.Goods.DTO.DTO.PurchaseOrderItem.PurchaseOrderItemSearchDTO>, 
    Inventorization.Goods.BL.SearchProviders.PurchaseOrderItemSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.PurchaseOrderItem.CreatePurchaseOrderItemDTO>, 
    Inventorization.Goods.BL.Validators.CreatePurchaseOrderItemValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.PurchaseOrderItem.UpdatePurchaseOrderItemDTO>, 
    Inventorization.Goods.BL.Validators.UpdatePurchaseOrderItemValidator>();

// GoodSupplier junction entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.BL.Entities.GoodSupplier, Inventorization.Goods.DTO.DTO.GoodSupplier.GoodSupplierDetailsDTO>, 
    Inventorization.Goods.BL.Mappers.GoodSupplierMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.BL.Entities.GoodSupplier, Inventorization.Goods.DTO.DTO.GoodSupplier.CreateGoodSupplierDTO>, 
    Inventorization.Goods.BL.Creators.GoodSupplierCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.BL.Entities.GoodSupplier, Inventorization.Goods.DTO.DTO.GoodSupplier.UpdateGoodSupplierDTO>, 
    Inventorization.Goods.BL.Modifiers.GoodSupplierModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.BL.Entities.GoodSupplier, Inventorization.Goods.DTO.DTO.GoodSupplier.GoodSupplierSearchDTO>, 
    Inventorization.Goods.BL.SearchProviders.GoodSupplierSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.GoodSupplier.CreateGoodSupplierDTO>, 
    Inventorization.Goods.BL.Validators.CreateGoodSupplierValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.GoodSupplier.UpdateGoodSupplierDTO>, 
    Inventorization.Goods.BL.Validators.UpdateGoodSupplierValidator>();

// ===== Property Accessors (Keyed Services) =====
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.BL.Entities.Category, Guid?>>(
    sp => new Inventorization.Goods.BL.PropertyAccessors.CategoryParentIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.BL.Entities.Good, Guid?>>(
    sp => new Inventorization.Goods.BL.PropertyAccessors.GoodCategoryIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.BL.Entities.StockLocation, Guid>>(
    sp => new Inventorization.Goods.BL.PropertyAccessors.StockLocationWarehouseIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.BL.Entities.StockItem, Guid>>(
    sp => new Inventorization.Goods.BL.PropertyAccessors.StockItemLocationIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.BL.Entities.PurchaseOrder, Guid>>(
    sp => new Inventorization.Goods.BL.PropertyAccessors.PurchaseOrderSupplierIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.BL.Entities.PurchaseOrderItem, Guid>>(
    sp => new Inventorization.Goods.BL.PropertyAccessors.PurchaseOrderItemOrderIdAccessor());

// Junction entity property accessors
builder.Services.AddScoped<IEntityIdPropertyAccessor<Inventorization.Goods.BL.Entities.GoodSupplier>, 
    Inventorization.Goods.BL.PropertyAccessors.GoodSupplierEntityIdAccessor>();
builder.Services.AddScoped<IRelatedEntityIdPropertyAccessor<Inventorization.Goods.BL.Entities.GoodSupplier>, 
    Inventorization.Goods.BL.PropertyAccessors.GoodSupplierRelatedEntityIdAccessor>();

// ===== Relationship Managers =====
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.CategoryGoodRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.CategorySubcategoryRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.WarehouseStockLocationRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.StockLocationStockItemRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.GoodStockItemRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.SupplierPurchaseOrderRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.PurchaseOrderPurchaseOrderItemRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.GoodPurchaseOrderItemRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.GoodSupplierRelationshipManager>();

// ===== Data Services =====
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.IGoodDataService, 
    Inventorization.Goods.BL.DataServices.GoodDataService>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.ICategoryDataService, 
    Inventorization.Goods.BL.DataServices.CategoryDataService>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.ISupplierDataService, 
    Inventorization.Goods.BL.DataServices.SupplierDataService>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.IWarehouseDataService, 
    Inventorization.Goods.BL.DataServices.WarehouseDataService>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.IStockLocationDataService, 
    Inventorization.Goods.BL.DataServices.StockLocationDataService>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.IStockItemDataService, 
    Inventorization.Goods.BL.DataServices.StockItemDataService>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.IPurchaseOrderDataService, 
    Inventorization.Goods.BL.DataServices.PurchaseOrderDataService>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.IPurchaseOrderItemDataService, 
    Inventorization.Goods.BL.DataServices.PurchaseOrderItemDataService>();
builder.Services.AddScoped<Inventorization.Goods.BL.DataServices.IGoodSupplierDataService, 
    Inventorization.Goods.BL.DataServices.GoodSupplierDataService>();

// ===== Controllers & Swagger =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Inventorization.Goods.API", 
        Version = "v1",
        Description = "Goods Bounded Context API - Manages goods/products in the inventory system"
    });
    
    // JWT Bearer token support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===== Middleware Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventorization.Goods.API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
