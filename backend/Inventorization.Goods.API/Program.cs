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

builder.Services.AddDbContext<Inventorization.Goods.Domain.DbContexts.GoodsDbContext>(options =>
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
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<Inventorization.Goods.Domain.DbContexts.GoodsDbContext>());

// Generic repository registration
builder.Services.AddScoped(typeof(IRepository<>), typeof(Inventorization.Base.DataAccess.BaseRepository<>));

// UnitOfWork registration
builder.Services.AddScoped<Inventorization.Goods.Domain.DataAccess.IGoodsUnitOfWork, Inventorization.Goods.Domain.DataAccess.GoodsUnitOfWork>();
builder.Services.AddScoped<Inventorization.Base.DataAccess.IUnitOfWork>(sp => sp.GetRequiredService<Inventorization.Goods.Domain.DataAccess.IGoodsUnitOfWork>());

// ===== Mappers, Creators, Modifiers, SearchProviders, Validators =====
// Good entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.Domain.Entities.Good, Inventorization.Goods.DTO.DTO.Good.GoodDetailsDTO>, 
    Inventorization.Goods.Domain.Mappers.GoodMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.Domain.Entities.Good, Inventorization.Goods.DTO.DTO.Good.CreateGoodDTO>, 
    Inventorization.Goods.Domain.Creators.GoodCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.Domain.Entities.Good, Inventorization.Goods.DTO.DTO.Good.UpdateGoodDTO>, 
    Inventorization.Goods.Domain.Modifiers.GoodModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.Domain.Entities.Good, Inventorization.Goods.DTO.DTO.Good.GoodSearchDTO>, 
    Inventorization.Goods.Domain.SearchProviders.GoodSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Good.CreateGoodDTO>, 
    Inventorization.Goods.Domain.Validators.CreateGoodValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Good.UpdateGoodDTO>, 
    Inventorization.Goods.Domain.Validators.UpdateGoodValidator>();

// Category entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.Domain.Entities.Category, Inventorization.Goods.DTO.DTO.Category.CategoryDetailsDTO>, 
    Inventorization.Goods.Domain.Mappers.CategoryMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.Domain.Entities.Category, Inventorization.Goods.DTO.DTO.Category.CreateCategoryDTO>, 
    Inventorization.Goods.Domain.Creators.CategoryCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.Domain.Entities.Category, Inventorization.Goods.DTO.DTO.Category.UpdateCategoryDTO>, 
    Inventorization.Goods.Domain.Modifiers.CategoryModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.Domain.Entities.Category, Inventorization.Goods.DTO.DTO.Category.CategorySearchDTO>, 
    Inventorization.Goods.Domain.SearchProviders.CategorySearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Category.CreateCategoryDTO>, 
    Inventorization.Goods.Domain.Validators.CreateCategoryValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Category.UpdateCategoryDTO>, 
    Inventorization.Goods.Domain.Validators.UpdateCategoryValidator>();

// Supplier entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.Domain.Entities.Supplier, Inventorization.Goods.DTO.DTO.Supplier.SupplierDetailsDTO>, 
    Inventorization.Goods.Domain.Mappers.SupplierMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.Domain.Entities.Supplier, Inventorization.Goods.DTO.DTO.Supplier.CreateSupplierDTO>, 
    Inventorization.Goods.Domain.Creators.SupplierCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.Domain.Entities.Supplier, Inventorization.Goods.DTO.DTO.Supplier.UpdateSupplierDTO>, 
    Inventorization.Goods.Domain.Modifiers.SupplierModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.Domain.Entities.Supplier, Inventorization.Goods.DTO.DTO.Supplier.SupplierSearchDTO>, 
    Inventorization.Goods.Domain.SearchProviders.SupplierSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Supplier.CreateSupplierDTO>, 
    Inventorization.Goods.Domain.Validators.CreateSupplierValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Supplier.UpdateSupplierDTO>, 
    Inventorization.Goods.Domain.Validators.UpdateSupplierValidator>();

// Warehouse entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.Domain.Entities.Warehouse, Inventorization.Goods.DTO.DTO.Warehouse.WarehouseDetailsDTO>, 
    Inventorization.Goods.Domain.Mappers.WarehouseMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.Domain.Entities.Warehouse, Inventorization.Goods.DTO.DTO.Warehouse.CreateWarehouseDTO>, 
    Inventorization.Goods.Domain.Creators.WarehouseCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.Domain.Entities.Warehouse, Inventorization.Goods.DTO.DTO.Warehouse.UpdateWarehouseDTO>, 
    Inventorization.Goods.Domain.Modifiers.WarehouseModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.Domain.Entities.Warehouse, Inventorization.Goods.DTO.DTO.Warehouse.WarehouseSearchDTO>, 
    Inventorization.Goods.Domain.SearchProviders.WarehouseSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Warehouse.CreateWarehouseDTO>, 
    Inventorization.Goods.Domain.Validators.CreateWarehouseValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.Warehouse.UpdateWarehouseDTO>, 
    Inventorization.Goods.Domain.Validators.UpdateWarehouseValidator>();

// StockLocation entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.Domain.Entities.StockLocation, Inventorization.Goods.DTO.DTO.StockLocation.StockLocationDetailsDTO>, 
    Inventorization.Goods.Domain.Mappers.StockLocationMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.Domain.Entities.StockLocation, Inventorization.Goods.DTO.DTO.StockLocation.CreateStockLocationDTO>, 
    Inventorization.Goods.Domain.Creators.StockLocationCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.Domain.Entities.StockLocation, Inventorization.Goods.DTO.DTO.StockLocation.UpdateStockLocationDTO>, 
    Inventorization.Goods.Domain.Modifiers.StockLocationModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.Domain.Entities.StockLocation, Inventorization.Goods.DTO.DTO.StockLocation.StockLocationSearchDTO>, 
    Inventorization.Goods.Domain.SearchProviders.StockLocationSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.StockLocation.CreateStockLocationDTO>, 
    Inventorization.Goods.Domain.Validators.CreateStockLocationValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.StockLocation.UpdateStockLocationDTO>, 
    Inventorization.Goods.Domain.Validators.UpdateStockLocationValidator>();

// StockItem entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.Domain.Entities.StockItem, Inventorization.Goods.DTO.DTO.StockItem.StockItemDetailsDTO>, 
    Inventorization.Goods.Domain.Mappers.StockItemMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.Domain.Entities.StockItem, Inventorization.Goods.DTO.DTO.StockItem.CreateStockItemDTO>, 
    Inventorization.Goods.Domain.Creators.StockItemCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.Domain.Entities.StockItem, Inventorization.Goods.DTO.DTO.StockItem.UpdateStockItemDTO>, 
    Inventorization.Goods.Domain.Modifiers.StockItemModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.Domain.Entities.StockItem, Inventorization.Goods.DTO.DTO.StockItem.StockItemSearchDTO>, 
    Inventorization.Goods.Domain.SearchProviders.StockItemSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.StockItem.CreateStockItemDTO>, 
    Inventorization.Goods.Domain.Validators.CreateStockItemValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.StockItem.UpdateStockItemDTO>, 
    Inventorization.Goods.Domain.Validators.UpdateStockItemValidator>();

// PurchaseOrder entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.Domain.Entities.PurchaseOrder, Inventorization.Goods.DTO.DTO.PurchaseOrder.PurchaseOrderDetailsDTO>, 
    Inventorization.Goods.Domain.Mappers.PurchaseOrderMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.Domain.Entities.PurchaseOrder, Inventorization.Goods.DTO.DTO.PurchaseOrder.CreatePurchaseOrderDTO>, 
    Inventorization.Goods.Domain.Creators.PurchaseOrderCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.Domain.Entities.PurchaseOrder, Inventorization.Goods.DTO.DTO.PurchaseOrder.UpdatePurchaseOrderDTO>, 
    Inventorization.Goods.Domain.Modifiers.PurchaseOrderModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.Domain.Entities.PurchaseOrder, Inventorization.Goods.DTO.DTO.PurchaseOrder.PurchaseOrderSearchDTO>, 
    Inventorization.Goods.Domain.SearchProviders.PurchaseOrderSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.PurchaseOrder.CreatePurchaseOrderDTO>, 
    Inventorization.Goods.Domain.Validators.CreatePurchaseOrderValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.PurchaseOrder.UpdatePurchaseOrderDTO>, 
    Inventorization.Goods.Domain.Validators.UpdatePurchaseOrderValidator>();

// PurchaseOrderItem entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.Domain.Entities.PurchaseOrderItem, Inventorization.Goods.DTO.DTO.PurchaseOrderItem.PurchaseOrderItemDetailsDTO>, 
    Inventorization.Goods.Domain.Mappers.PurchaseOrderItemMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.Domain.Entities.PurchaseOrderItem, Inventorization.Goods.DTO.DTO.PurchaseOrderItem.CreatePurchaseOrderItemDTO>, 
    Inventorization.Goods.Domain.Creators.PurchaseOrderItemCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.Domain.Entities.PurchaseOrderItem, Inventorization.Goods.DTO.DTO.PurchaseOrderItem.UpdatePurchaseOrderItemDTO>, 
    Inventorization.Goods.Domain.Modifiers.PurchaseOrderItemModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.Domain.Entities.PurchaseOrderItem, Inventorization.Goods.DTO.DTO.PurchaseOrderItem.PurchaseOrderItemSearchDTO>, 
    Inventorization.Goods.Domain.SearchProviders.PurchaseOrderItemSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.PurchaseOrderItem.CreatePurchaseOrderItemDTO>, 
    Inventorization.Goods.Domain.Validators.CreatePurchaseOrderItemValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.PurchaseOrderItem.UpdatePurchaseOrderItemDTO>, 
    Inventorization.Goods.Domain.Validators.UpdatePurchaseOrderItemValidator>();

// GoodSupplier junction entity abstractions
builder.Services.AddScoped<IMapper<Inventorization.Goods.Domain.Entities.GoodSupplier, Inventorization.Goods.DTO.DTO.GoodSupplier.GoodSupplierDetailsDTO>, 
    Inventorization.Goods.Domain.Mappers.GoodSupplierMapper>();
builder.Services.AddScoped<IEntityCreator<Inventorization.Goods.Domain.Entities.GoodSupplier, Inventorization.Goods.DTO.DTO.GoodSupplier.CreateGoodSupplierDTO>, 
    Inventorization.Goods.Domain.Creators.GoodSupplierCreator>();
builder.Services.AddScoped<IEntityModifier<Inventorization.Goods.Domain.Entities.GoodSupplier, Inventorization.Goods.DTO.DTO.GoodSupplier.UpdateGoodSupplierDTO>, 
    Inventorization.Goods.Domain.Modifiers.GoodSupplierModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Inventorization.Goods.Domain.Entities.GoodSupplier, Inventorization.Goods.DTO.DTO.GoodSupplier.GoodSupplierSearchDTO>, 
    Inventorization.Goods.Domain.SearchProviders.GoodSupplierSearchProvider>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.GoodSupplier.CreateGoodSupplierDTO>, 
    Inventorization.Goods.Domain.Validators.CreateGoodSupplierValidator>();
builder.Services.AddScoped<IValidator<Inventorization.Goods.DTO.DTO.GoodSupplier.UpdateGoodSupplierDTO>, 
    Inventorization.Goods.Domain.Validators.UpdateGoodSupplierValidator>();

// ===== Property Accessors (Keyed Services) =====
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.Domain.Entities.Category, Guid?>>(
    sp => new Inventorization.Goods.Domain.PropertyAccessors.CategoryParentIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.Domain.Entities.Good, Guid?>>(
    sp => new Inventorization.Goods.Domain.PropertyAccessors.GoodCategoryIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.Domain.Entities.StockLocation, Guid>>(
    sp => new Inventorization.Goods.Domain.PropertyAccessors.StockLocationWarehouseIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.Domain.Entities.StockItem, Guid>>(
    sp => new Inventorization.Goods.Domain.PropertyAccessors.StockItemLocationIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.Domain.Entities.PurchaseOrder, Guid>>(
    sp => new Inventorization.Goods.Domain.PropertyAccessors.PurchaseOrderSupplierIdAccessor());
builder.Services.AddScoped<IPropertyAccessor<Inventorization.Goods.Domain.Entities.PurchaseOrderItem, Guid>>(
    sp => new Inventorization.Goods.Domain.PropertyAccessors.PurchaseOrderItemOrderIdAccessor());

// Junction entity property accessors
builder.Services.AddScoped<IEntityIdPropertyAccessor<Inventorization.Goods.Domain.Entities.GoodSupplier>, 
    Inventorization.Goods.Domain.PropertyAccessors.GoodSupplierEntityIdAccessor>();
builder.Services.AddScoped<IRelatedEntityIdPropertyAccessor<Inventorization.Goods.Domain.Entities.GoodSupplier>, 
    Inventorization.Goods.Domain.PropertyAccessors.GoodSupplierRelatedEntityIdAccessor>();

// ===== Relationship Managers =====
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.CategoryGoodRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.CategorySubcategoryRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.WarehouseStockLocationRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.StockLocationStockItemRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.GoodStockItemRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.SupplierPurchaseOrderRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.PurchaseOrderPurchaseOrderItemRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.GoodPurchaseOrderItemRelationshipManager>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.GoodSupplierRelationshipManager>();

// ===== Data Services =====
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.IGoodDataService, 
    Inventorization.Goods.Domain.DataServices.GoodDataService>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.ICategoryDataService, 
    Inventorization.Goods.Domain.DataServices.CategoryDataService>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.ISupplierDataService, 
    Inventorization.Goods.Domain.DataServices.SupplierDataService>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.IWarehouseDataService, 
    Inventorization.Goods.Domain.DataServices.WarehouseDataService>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.IStockLocationDataService, 
    Inventorization.Goods.Domain.DataServices.StockLocationDataService>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.IStockItemDataService, 
    Inventorization.Goods.Domain.DataServices.StockItemDataService>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.IPurchaseOrderDataService, 
    Inventorization.Goods.Domain.DataServices.PurchaseOrderDataService>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.IPurchaseOrderItemDataService, 
    Inventorization.Goods.Domain.DataServices.PurchaseOrderItemDataService>();
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.IGoodSupplierDataService, 
    Inventorization.Goods.Domain.DataServices.GoodSupplierDataService>();

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
