using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Auth.Domain.DbContexts;
using Inventorization.Auth.Domain.DataAccess.UnitOfWork;
using Inventorization.Auth.Domain.DataAccess.Repositories;
using Inventorization.Auth.Domain.DataServices;
using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.Domain.Mappers;
using Inventorization.Auth.Domain.Creators;
using Inventorization.Auth.Domain.Modifiers;
using Inventorization.Auth.Domain.SearchProviders;
using Inventorization.Auth.Domain.Validators;
using Inventorization.Auth.Domain.Services.Abstractions;
using Inventorization.Auth.Domain.Services.Implementations;
using Inventorization.Auth.Domain.DataAccess.Seeding;
using Inventorization.Auth.DTO.DTO.User;
using Inventorization.Auth.DTO.DTO.Role;
using Inventorization.Auth.DTO.DTO.Permission;

var builder = WebApplication.CreateBuilder(args);

// ===== Database Configuration =====
var connectionString = builder.Configuration.GetConnectionString("AuthDatabase") 
    ?? throw new InvalidOperationException("Connection string 'AuthDatabase' not found.");

builder.Services.AddDbContext<AuthDbContext>(options =>
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
        ClockSkew = TimeSpan.Zero // No tolerance for token expiration
    };
});

builder.Services.AddAuthorization();

// ===== Repository & UnitOfWork Registration =====
// Concrete specialized repositories
builder.Services.AddScoped<Inventorization.Auth.Domain.Services.Abstractions.IUserRepository>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new Inventorization.Auth.Domain.DataAccess.Repositories.UserRepository(dbContext);
});
// Register DataAccess.Repositories interface as well (used by validators)
builder.Services.AddScoped<Inventorization.Auth.Domain.DataAccess.Repositories.IUserRepository>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new Inventorization.Auth.Domain.DataAccess.Repositories.UserRepository(dbContext);
});

builder.Services.AddScoped<Inventorization.Auth.Domain.Services.Abstractions.IRefreshTokenRepository>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new Inventorization.Auth.Domain.DataAccess.Repositories.RefreshTokenRepository(dbContext);
});

// Specific repositories for Role and Permission
builder.Services.AddScoped<IRepository<Role>>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new BaseRepository<Role>(dbContext);
});
builder.Services.AddScoped<IRepository<Permission>>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new BaseRepository<Permission>(dbContext);
});
// Register IRepository<User> for UserDataService
builder.Services.AddScoped<IRepository<User>>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new Inventorization.Auth.Domain.DataAccess.Repositories.UserRepository(dbContext);
});

// UnitOfWork
builder.Services.AddScoped<IAuthUnitOfWork, AuthUnitOfWork>();
builder.Services.AddScoped<Inventorization.Base.DataAccess.IUnitOfWork>(sp => sp.GetRequiredService<IAuthUnitOfWork>());

// ===== User Service Registration =====
builder.Services.AddScoped<IMapper<User, UserDetailsDTO>, UserMapper>();
builder.Services.AddScoped<IEntityCreator<User, CreateUserDTO>, UserCreator>();
builder.Services.AddScoped<IEntityModifier<User, UpdateUserDTO>, UserModifier>();
builder.Services.AddScoped<ISearchQueryProvider<User, UserSearchDTO>, UserSearchProvider>();
builder.Services.AddScoped<IValidator<CreateUserDTO>, CreateUserDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateUserDTO>, UpdateUserDtoValidator>();
builder.Services.AddScoped<IUserDataService, UserDataService>();

// ===== Role Service Registration =====
builder.Services.AddScoped<IMapper<Role, RoleDetailsDTO>, RoleMapper>();
builder.Services.AddScoped<IEntityCreator<Role, CreateRoleDTO>, RoleCreator>();
builder.Services.AddScoped<IEntityModifier<Role, UpdateRoleDTO>, RoleModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Role, RoleSearchDTO>, RoleSearchProvider>();
builder.Services.AddScoped<IValidator<CreateRoleDTO>, CreateRoleDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateRoleDTO>, UpdateRoleDtoValidator>();
builder.Services.AddScoped<IRoleDataService, RoleDataService>();

// ===== Permission Service Registration =====
builder.Services.AddScoped<IMapper<Permission, PermissionDetailsDTO>, PermissionMapper>();
builder.Services.AddScoped<IEntityCreator<Permission, CreatePermissionDTO>, PermissionCreator>();
builder.Services.AddScoped<IEntityModifier<Permission, UpdatePermissionDTO>, PermissionModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Permission, PermissionSearchDTO>, PermissionSearchProvider>();
builder.Services.AddScoped<IValidator<CreatePermissionDTO>, CreatePermissionDtoValidator>();
builder.Services.AddScoped<IValidator<UpdatePermissionDTO>, UpdatePermissionDtoValidator>();
builder.Services.AddScoped<IPermissionDataService, PermissionDataService>();

// ===== Password Hasher Registration =====
builder.Services.AddScoped<Inventorization.Base.Abstractions.IPasswordHasher, BcryptPasswordHasher>();

// ===== Authentication & Authorization Services =====
builder.Services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
builder.Services.AddScoped<Inventorization.Auth.Domain.Services.Abstractions.ITokenRotationService, TokenRotationService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<Inventorization.Auth.Domain.Services.Abstractions.IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// ===== Repository Implementations (using Service.Abstractions interfaces) =====
// Note: Actual implementations need to be created or BaseRepository<T> used
builder.Services.AddScoped<Inventorization.Auth.Domain.Services.Abstractions.IUserRepository>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new Inventorization.Auth.Domain.DataAccess.Repositories.UserRepository(dbContext);
});
builder.Services.AddScoped<Inventorization.Auth.Domain.Services.Abstractions.IRefreshTokenRepository>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new Inventorization.Auth.Domain.DataAccess.Repositories.RefreshTokenRepository(dbContext);
});

// ===== Controllers & Swagger =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Inventorization Auth API", 
        Version = "v1",
        Description = "Authentication and authorization microservice"
    });

    // JWT Bearer configuration for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
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

// ===== CORS Configuration (for development) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ===== Database Seeding =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database seeding...");
        var context = services.GetRequiredService<AuthDbContext>();
        var seederLogger = services.GetRequiredService<ILogger<AuthDbSeeder>>();
        var passwordHasher = services.GetRequiredService<Inventorization.Base.Abstractions.IPasswordHasher>();
        var seeder = new AuthDbSeeder(context, seederLogger, passwordHasher);
        await seeder.SeedAsync();
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ===== Middleware Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
        c.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
