using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Inventorization.Base.Models;

namespace Inventorization.Base.DataAccess;

/// <summary>
/// Base entity configuration for all entities inheriting from BaseEntity.
/// Handles common configuration like primary key, table naming, and common indexes.
/// </summary>
/// <typeparam name="TEntity">Entity type (must inherit from BaseEntity)</typeparam>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Table name: pluralize entity name
        var tableName = GetTableName();
        builder.ToTable(tableName);
        
        // Primary key (inherited from BaseEntity)
        builder.HasKey(e => e.Id);
        
        // Call derived class configuration
        // Note: Derived classes should explicitly configure their own indexes
        // for properties like CreatedAt, IsActive, etc. for type safety
        ConfigureEntity(builder);
    }
    
    /// <summary>
    /// Override this method to configure entity-specific properties and relationships
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
    
    /// <summary>
    /// Gets the table name for the entity. Override to customize table naming.
    /// Default: simple pluralization (append 's', handle 'y' → 'ies', etc.)
    /// </summary>
    protected virtual string GetTableName()
    {
        var entityName = typeof(TEntity).Name;
        
        // Simple pluralization rules
        if (entityName.EndsWith("s") || entityName.EndsWith("x") || entityName.EndsWith("ch") || entityName.EndsWith("sh"))
            return entityName + "es";
        
        if (entityName.EndsWith("y") && !IsVowel(entityName[^2]))
            return entityName[..^1] + "ies";
        
        return entityName + "s";
    }
    
    /// <summary>
    /// Checks if a character is a vowel
    /// </summary>
    private static bool IsVowel(char c)
    {
        return "aeiouAEIOU".Contains(c);
    }
}
