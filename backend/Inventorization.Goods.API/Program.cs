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
builder.Services.AddScoped(typeof(IRepository<>), sp =>
{
    var dbContext = sp.GetRequiredService<Inventorization.Goods.Domain.DbContexts.GoodsDbContext>();
    return Activator.CreateInstance(typeof(Inventorization.Base.DataAccess.BaseRepository<>).MakeGenericType(sp.GetType().GetGenericArguments()[0]), dbContext)!;
});
builder.Services.AddScoped<IRepository<Inventorization.Goods.Domain.Entities.Good>>(sp =>
{
    var dbContext = sp.GetRequiredService<Inventorization.Goods.Domain.DbContexts.GoodsDbContext>();
    return new Inventorization.Base.DataAccess.BaseRepository<Inventorization.Goods.Domain.Entities.Good>(dbContext);
});
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

// ===== Data Services =====
builder.Services.AddScoped<Inventorization.Goods.Domain.DataServices.IGoodDataService, 
    Inventorization.Goods.Domain.DataServices.GoodDataService>();

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
