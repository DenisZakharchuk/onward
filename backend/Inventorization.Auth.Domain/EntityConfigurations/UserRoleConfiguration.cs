using Inventorization.Base.DataAccess;
using Inventorization.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Auth.Domain.EntityConfigurations;

public class UserRoleConfiguration : JunctionEntityConfiguration<UserRole, User, Role>
{
    public UserRoleConfiguration() : base(DataModelRelationships.UserRoles)
    {
    }
    
    protected override void ConfigureJunctionEntity(EntityTypeBuilder<UserRole> builder)
    {
        // Configure relationships - CASCADE DELETE PROHIBITED to prevent accidental data loss
        builder.HasOne(e => e.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
