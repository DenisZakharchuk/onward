using Inventorization.Base.DataAccess;
using Inventorization.Goods.BL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Goods.BL.EntityConfigurations;

public class GoodSupplierConfiguration : JunctionEntityConfiguration<GoodSupplier, Good, Supplier>
{
    public GoodSupplierConfiguration() : base(DataModelRelationships.GoodSuppliers)
    {
    }
    
    protected override void ConfigureJunctionEntity(EntityTypeBuilder<GoodSupplier> builder)
    {
        // Ignore computed properties (they're just aliases for base class properties)
        builder.Ignore(e => e.GoodId);
        builder.Ignore(e => e.SupplierId);
        
        // Configure metadata columns
        builder.Property(e => e.SupplierPrice)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.LeadTimeDays)
            .IsRequired();
        
        builder.HasIndex(e => e.IsPreferred);
        
        // Configure relationships using base class properties
        builder.HasOne(e => e.Good)
            .WithMany(g => g.GoodSuppliers)
            .HasForeignKey(e => e.EntityId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Supplier)
            .WithMany(s => s.GoodSuppliers)
            .HasForeignKey(e => e.RelatedEntityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
