using Inventorization.Base.DataAccess;
using Inventorization.Goods.BL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Goods.BL.EntityConfigurations;

public class StockItemConfiguration : BaseEntityConfiguration<StockItem>
{
    protected override void ConfigureEntity(EntityTypeBuilder<StockItem> builder)
    {
        builder.Property(e => e.Quantity)
            .IsRequired();
        
        builder.Property(e => e.BatchNumber)
            .HasMaxLength(100);
        
        builder.Property(e => e.SerialNumber)
            .HasMaxLength(100);
        
        // Composite unique index on GoodId + StockLocationId
        builder.HasIndex(e => new { e.GoodId, e.StockLocationId })
            .IsUnique();
        
        builder.HasIndex(e => e.BatchNumber);
        builder.HasIndex(e => e.SerialNumber);
        builder.HasIndex(e => e.ExpiryDate);
        
        // Relationship: StockItem belongs to Good (required)
        builder.HasOne(e => e.Good)
            .WithMany(g => g.StockItems)
            .HasForeignKey(e => e.GoodId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relationship: StockItem belongs to StockLocation (required)
        builder.HasOne(e => e.StockLocation)
            .WithMany(sl => sl.StockItems)
            .HasForeignKey(e => e.StockLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
