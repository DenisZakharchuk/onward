using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// ===== Database Configuration =====
var connectionString = builder.Configuration.GetConnectionString("CommerceDatabase") 
    ?? throw new InvalidOperationException("Connection string 'CommerceDatabase' not found.");

builder.Services.AddDbContext<Inventorization.Commerce.Domain.DbContexts.CommerceDbContext>(options =>
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
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<Inventorization.Commerce.Domain.DbContexts.CommerceDbContext>());
builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<Inventorization.Commerce.Domain.DataAccess.ICommerceUnitOfWork, Inventorization.Commerce.Domain.DataAccess.CommerceUnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<Inventorization.Commerce.Domain.DataAccess.ICommerceUnitOfWork>());

// ===== Data Services =====
builder.Services.AddScoped<Inventorization.Commerce.Domain.DataServices.IProductDataService, Inventorization.Commerce.Domain.DataServices.ProductDataService>();
builder.Services.AddScoped<Inventorization.Commerce.Domain.DataServices.ICategoryDataService, Inventorization.Commerce.Domain.DataServices.CategoryDataService>();
builder.Services.AddScoped<Inventorization.Commerce.Domain.DataServices.ICustomerDataService, Inventorization.Commerce.Domain.DataServices.CustomerDataService>();
builder.Services.AddScoped<Inventorization.Commerce.Domain.DataServices.IAddressDataService, Inventorization.Commerce.Domain.DataServices.AddressDataService>();
builder.Services.AddScoped<Inventorization.Commerce.Domain.DataServices.IOrderDataService, Inventorization.Commerce.Domain.DataServices.OrderDataService>();
builder.Services.AddScoped<Inventorization.Commerce.Domain.DataServices.IOrderItemDataService, Inventorization.Commerce.Domain.DataServices.OrderItemDataService>();

// ===== Controllers & Swagger =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Commerce API",
        Version = "v1",
        Description = "E-commerce orders and customer management API"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
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

// ===== HTTP Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Commerce API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
