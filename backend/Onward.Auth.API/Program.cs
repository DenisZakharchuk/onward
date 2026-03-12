using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Onward.Auth.API.GrpcServices;
using Onward.Auth.BL.DataAccess.Seeding;
using Onward.Auth.BL.DbContexts;
using Onward.Auth.BL.Extensions;
using Onward.Base.Abstractions;
using Onward.Base.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ===== Database Configuration =====
var connectionString = builder.Configuration.GetConnectionString("AuthDatabase")
    ?? throw new InvalidOperationException("Connection string 'AuthDatabase' not found.");

// ===== Business-layer services (DbContext, repos, mappers, domain services, etc.) =====
builder.Services.AddOnwardAuthBusinessServices(opt => opt.UseNpgsql(connectionString));

// ===== JWT authentication + authorization =====
builder.Services.AddOnwardJwtAuth(builder.Configuration);

// ===== gRPC =====
builder.Services.AddGrpc();

// ===== Controllers & Swagger =====
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Onward Auth API",
        Version = "v1",
        Description = "Authentication and authorization microservice"
    });

    c.AddOnwardJwtSecurity();
});

// ===== CORS Configuration =====
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
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
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

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseOnwardAuth();
app.MapControllers();
app.MapGrpcService<AuthIntrospectionGrpcService>();

app.Run();
