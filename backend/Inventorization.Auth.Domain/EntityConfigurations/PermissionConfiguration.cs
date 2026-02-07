using Inventorization.Base.DataAccess;
using Inventorization.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Auth.Domain.EntityConfigurations;

public class PermissionConfiguration : BaseEntityConfiguration<Permission>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(e => e.Name)
            .IsUnique();
        
        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        builder.Property(e => e.Resource)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(e => new { e.Resource, e.Action })
            .IsUnique();
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
    }
}
