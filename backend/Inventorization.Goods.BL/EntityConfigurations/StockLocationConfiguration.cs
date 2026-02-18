using Inventorization.Base.DataAccess;
using Inventorization.Goods.BL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Goods.BL.EntityConfigurations;

public class StockLocationConfiguration : BaseEntityConfiguration<StockLocation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<StockLocation> builder)
    {
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Aisle)
            .HasMaxLength(50);
        
        builder.Property(e => e.Shelf)
            .HasMaxLength(50);
        
        builder.Property(e => e.Bin)
            .HasMaxLength(50);
        
        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        // Composite unique index on WarehouseId + Code
        builder.HasIndex(e => new { e.WarehouseId, e.Code })
            .IsUnique();
        
        // Relationship: StockLocation belongs to Warehouse (required)
        builder.HasOne(e => e.Warehouse)
            .WithMany(w => w.StockLocations)
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
