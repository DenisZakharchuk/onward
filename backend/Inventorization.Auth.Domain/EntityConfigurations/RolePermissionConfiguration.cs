using Inventorization.Base.DataAccess;
using Inventorization.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Auth.Domain.EntityConfigurations;

public class RolePermissionConfiguration : JunctionEntityConfiguration<RolePermission, Role, Permission>
{
    public RolePermissionConfiguration() : base(DataModelRelationships.RolePermissions)
    {
    }
    
    protected override void ConfigureJunctionEntity(EntityTypeBuilder<RolePermission> builder)
    {
        // Configure relationships - CASCADE DELETE PROHIBITED to prevent accidental data loss
        builder.HasOne(e => e.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(e => e.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
