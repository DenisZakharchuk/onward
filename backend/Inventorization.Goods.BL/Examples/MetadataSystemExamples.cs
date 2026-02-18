/*
 * EXAMPLE FILE - NOT FOR PRODUCTION USE
 * 
 * This file demonstrates how to use DataModelMetadata for:
 * 1. EF Core entity configuration
 * 2. DTO validation
 * 3. Generic metadata-driven patterns
 * 
 * Copy these patterns when implementing new entities.
 */

using Inventorization.Base.Abstractions;
using Inventorization.Base.DTOs;
using Inventorization.Base.Models;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.Good;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Goods.BL.Examples;

#region EF Configuration Examples

/// <summary>
/// EXAMPLE 1: Metadata-driven EF configuration
/// All property configuration comes from metadata, relationships configured manually
/// </summary>
public class MetadataDrivenEntityConfiguration : IEntityTypeConfiguration<Good>
{
    public void Configure(EntityTypeBuilder<Good> builder)
    {
        // Single line configures all properties from metadata!
        builder.ApplyMetadata(DataModelMetadata.Good);
        
        // Manual relationship configuration
        builder.HasOne(g => g.Category)
            .WithMany(c => c.Goods)
            .HasForeignKey(g => g.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// EXAMPLE 2: Hybrid approach - metadata + manual overrides
/// </summary>
public class HybridEntityConfiguration : IEntityTypeConfiguration<Good>
{
    public void Configure(EntityTypeBuilder<Good> builder)
    {
        // Apply metadata configuration
        builder.ApplyMetadata(DataModelMetadata.Good);
        
        // Override specific configurations if needed
        builder.Property(g => g.Description)
            .HasMaxLength(2000); // Override metadata maxLength
        
        // Add relationships
        builder.HasOne(g => g.Category)
            .WithMany(c => c.Goods)
            .HasForeignKey(g => g.CategoryId);
    }
}

#endregion

#region Validation Examples

/// <summary>
/// EXAMPLE 3: Pure metadata-driven validation
/// </summary>
public class PureMetadataValidator : IValidator<CreateGoodDTO>
{
    public async Task<Inventorization.Base.Abstractions.ValidationResult> ValidateAsync(
        CreateGoodDTO dto, 
        CancellationToken cancellationToken = default)
    {
        // Use metadata to validate
        var result = DataModelMetadata.Good.ValidateAgainstMetadata(
            dto,
            nameof(dto.Name),
            nameof(dto.Sku),
            nameof(dto.UnitPrice),
            nameof(dto.QuantityInStock));

        if (!result.IsValid)
        {
            return Inventorization.Base.Abstractions.ValidationResult.WithErrors(
                result.Errors.ToArray());
        }

        return await Task.FromResult(Inventorization.Base.Abstractions.ValidationResult.Ok());
    }
}

/// <summary>
/// EXAMPLE 4: Metadata + custom business rules
/// </summary>
public class HybridValidator : IValidator<UpdateGoodDTO>
{
    public async Task<Inventorization.Base.Abstractions.ValidationResult> ValidateAsync(
        UpdateGoodDTO dto, 
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // 1. Metadata validation (structural rules)
        var metadataResult = DataModelMetadata.Good.ValidateAgainstMetadata(dto);
        if (!metadataResult.IsValid)
        {
            errors.AddRange(metadataResult.Errors);
        }

        // 2. Custom business rules
        if (dto.UnitPrice < 0)
        {
            errors.Add("Unit price cannot be negative");
        }

        if (dto.QuantityInStock < 0)
        {
            errors.Add("Quantity in stock cannot be negative");
        }

        // Return result
        if (errors.Any())
        {
            return Inventorization.Base.Abstractions.ValidationResult.WithErrors(
                errors.ToArray());
        }

        return await Task.FromResult(Inventorization.Base.Abstractions.ValidationResult.Ok());
    }
}

/// <summary>
/// EXAMPLE 5: Generic metadata validator (reusable)
/// </summary>
public class GenericMetadataValidator<TEntity, TDto> : IValidator<TDto>
    where TEntity : class
    where TDto : class
{
    private readonly IDataModelMetadata<TEntity> _metadata;

    public GenericMetadataValidator(IDataModelMetadata<TEntity> metadata)
    {
        _metadata = metadata;
    }

    public async Task<Inventorization.Base.Abstractions.ValidationResult> ValidateAsync(
        TDto dto, 
        CancellationToken cancellationToken = default)
    {
        var result = _metadata.ValidateAgainstMetadata(dto);

        if (!result.IsValid)
        {
            return Inventorization.Base.Abstractions.ValidationResult.WithErrors(
                result.Errors.ToArray());
        }

        return await Task.FromResult(Inventorization.Base.Abstractions.ValidationResult.Ok());
    }
}

#endregion

#region Usage Examples

public static class MetadataSystemUsageExamples
{
    /// <summary>
    /// Example: Query metadata at runtime
    /// </summary>
    public static void QueryMetadataExample()
    {
        // Get metadata for Good entity
        var goodMetadata = DataModelMetadata.Good;
        
        Console.WriteLine($"Entity: {goodMetadata.EntityName}");
        Console.WriteLine($"Table: {goodMetadata.TableName}");
        Console.WriteLine($"Properties: {goodMetadata.Properties.Count}");
        
        // Access specific property metadata
        var nameProperty = goodMetadata.Properties["Name"];
        Console.WriteLine($"Name MaxLength: {nameProperty.MaxLength}");
        Console.WriteLine($"Name Required: {nameProperty.IsRequired}");
        
        // List all required properties
        var requiredProps = goodMetadata.Properties
            .Where(p => p.Value.IsRequired)
            .Select(p => p.Key);
        Console.WriteLine($"Required: {string.Join(", ", requiredProps)}");
    }

    /// <summary>
    /// Example: Use generic validator
    /// </summary>
    public static async Task ValidateWithMetadataExample()
    {
        var validator = new GenericMetadataValidator<Good, CreateGoodDTO>(
            DataModelMetadata.Good);

        var dto = new CreateGoodDTO
        {
            Name = "Test Product",
            Sku = "TEST-001",
            UnitPrice = 99.99m,
            QuantityInStock = 10
        };

        var result = await validator.ValidateAsync(dto);
        
        if (result.IsValid)
        {
            Console.WriteLine("Validation passed!");
        }
        else
        {
            Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
        }
    }

    /// <summary>
    /// Example: Generate API documentation from metadata
    /// </summary>
    public static void GenerateDocumentationExample()
    {
        var metadata = DataModelMetadata.Good;
        
        Console.WriteLine($"## {metadata.DisplayName}");
        Console.WriteLine(metadata.Description);
        Console.WriteLine();
        Console.WriteLine("### Properties");
        
        foreach (var (name, prop) in metadata.Properties)
        {
            Console.WriteLine($"- **{prop.DisplayName}** ({prop.PropertyType.Name})");
            if (prop.IsRequired) Console.WriteLine($"  - Required");
            if (prop.MaxLength.HasValue) Console.WriteLine($"  - Max Length: {prop.MaxLength}");
            if (prop.Description != null) Console.WriteLine($"  - {prop.Description}");
        }
    }
}

#endregion
