using Inventorization.Base.DataAccess;
using Inventorization.Goods.BL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Goods.BL.EntityConfigurations;

public class PurchaseOrderItemConfiguration : BaseEntityConfiguration<PurchaseOrderItem>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.Property(e => e.Quantity)
            .IsRequired();
        
        builder.Property(e => e.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.ReceivedQuantity)
            .IsRequired();
        
        builder.Property(e => e.Notes)
            .HasMaxLength(500);
        
        builder.HasIndex(e => e.PurchaseOrderId);
        builder.HasIndex(e => e.GoodId);
        
        // Relationship: PurchaseOrderItem belongs to PurchaseOrder (required)
        builder.HasOne(e => e.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(e => e.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship: PurchaseOrderItem belongs to Good (required)
        builder.HasOne(e => e.Good)
            .WithMany(g => g.PurchaseOrderItems)
            .HasForeignKey(e => e.GoodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
