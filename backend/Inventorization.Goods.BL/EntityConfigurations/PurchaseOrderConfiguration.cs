using Inventorization.Base.DataAccess;
using Inventorization.Goods.BL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Goods.BL.EntityConfigurations;

public class PurchaseOrderConfiguration : BaseEntityConfiguration<PurchaseOrder>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.Property(e => e.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(e => e.OrderNumber)
            .IsUnique();
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(e => e.Notes)
            .HasMaxLength(1000);
        
        builder.HasIndex(e => e.OrderDate);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.SupplierId);
        
        // Relationship: PurchaseOrder belongs to Supplier (required)
        builder.HasOne(e => e.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
