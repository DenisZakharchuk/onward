using Onward.Base.DataAccess;
using Onward.Auth.BL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Onward.Auth.BL.EntityConfigurations;

public class RoleConfiguration : BaseEntityConfiguration<Role>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Role> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(e => e.Name)
            .IsUnique();
        
        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
    }
}
