using Inventorization.Goods.Domain.Entities;

namespace Inventorization.Goods.Domain.DbContexts;

/// <summary>
/// Database context for the Goods bounded context
/// </summary>
public class GoodsDbContext : DbContext
{
    public GoodsDbContext(DbContextOptions<GoodsDbContext> options) : base(options)
    {
    }
    
    public DbSet<Good> Goods => Set<Good>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Good entity
        modelBuilder.Entity<Good>(entity =>
        {
            entity.ToTable("Goods");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.Sku)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.HasIndex(e => e.Sku)
                .IsUnique();
            
            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2);
            
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(50);
            
            entity.Property(e => e.IsActive)
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
