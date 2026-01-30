using InventorySystem.API.GraphQL;
using InventorySystem.AuditLog.Abstractions;
using InventorySystem.AuditLog.Services;
using InventorySystem.Business.Abstractions;
using InventorySystem.Business.Services;
using InventorySystem.DataAccess.Abstractions;
using InventorySystem.DataAccess.Repositories;
using MongoDB.Driver;

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

// Register business services
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
