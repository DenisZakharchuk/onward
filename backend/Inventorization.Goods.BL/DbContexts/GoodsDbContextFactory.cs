using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Inventorization.Goods.BL.DbContexts;

/// <summary>
/// Design-time factory for GoodsDbContext to support EF Core migrations
/// </summary>
public class GoodsDbContextFactory : IDesignTimeDbContextFactory<GoodsDbContext>
{
    public GoodsDbContext CreateDbContext(string[] args)
    {
        // Build configuration from the API project's appsettings
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Inventorization.Goods.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<GoodsDbContext>();
        var connectionString = configuration.GetConnectionString("GoodsDatabase");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'GoodsDatabase' not found.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new GoodsDbContext(optionsBuilder.Options);
    }
}
