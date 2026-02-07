using Inventorization.Base.DataAccess;
using Inventorization.Goods.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Goods.Domain.EntityConfigurations;

public class GoodSupplierConfiguration : JunctionEntityConfiguration<GoodSupplier, Good, Supplier>
{
    public GoodSupplierConfiguration() : base(DataModelRelationships.GoodSuppliers)
    {
    }
    
    protected override void ConfigureJunctionEntity(EntityTypeBuilder<GoodSupplier> builder)
    {
        // Configure metadata columns
        builder.Property(e => e.SupplierPrice)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.LeadTimeDays)
            .IsRequired();
        
        builder.HasIndex(e => e.IsPreferred);
        
        // Configure relationships
        builder.HasOne(e => e.Good)
            .WithMany(g => g.GoodSuppliers)
            .HasForeignKey(e => e.GoodId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Supplier)
            .WithMany(s => s.GoodSuppliers)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
