/**
 * Configuration generator - creates EF Core entity configurations
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property } from '../models/DataModel';
import { TypeMapper } from '../utils/TypeMapper';
import * as path from 'path';

export class ConfigurationGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const configurationsDir = `${baseNamespace}.${contextName}.Domain/EntityConfigurations`;

    for (const entity of model.entities) {
      if (entity.isJunction) {
        // Junction entities need special configuration
        await this.generateJunctionConfiguration(entity, configurationsDir, namespace, baseNamespace, model);
      } else {
        await this.generateRegularConfiguration(entity, configurationsDir, namespace, baseNamespace, model);
      }
    }
  }

  private async generateRegularConfiguration(
    entity: Entity,
    configurationsDir: string,
    namespace: string,
    baseNamespace: string,
    model: DataModel
  ): Promise<void> {
    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      tableName: entity.tableName || this.pluralize(entity.name),
      schema: entity.schema,
      propertyConfigurations: this.getPropertyConfigurations(entity.properties),
      indexes: this.getIndexConfigurations(entity),
      relationships: this.getRelationshipConfigurations(entity, model),
    };

    const filePath = path.join(configurationsDir, `${entity.name}Configuration.cs`);
    await this.writeRenderedTemplate('entity-configuration.generated.cs.hbs', context, filePath, true);
  }

  private async generateJunctionConfiguration(
    entity: Entity,
    configurationsDir: string,
    namespace: string,
    baseNamespace: string,
    _model: DataModel
  ): Promise<void> {
    if (!entity.junctionMetadata) {
      throw new Error(`Junction entity ${entity.name} missing junctionMetadata`);
    }

    // For junction entities, we use JunctionEntityConfiguration base class
    const context = {
      baseNamespace,
      namespace,
      junctionName: entity.name,
      leftEntity: entity.junctionMetadata.leftEntity,
      rightEntity: entity.junctionMetadata.rightEntity,
      tableName: entity.tableName || this.pluralize(entity.name),
      schema: entity.schema,
      propertyConfigurations: this.getPropertyConfigurations(
        entity.properties.filter((p) => !p.isForeignKey && p.name !== 'Id')
      ),
      hasRelationships: true,
    };

    const filePath = path.join(configurationsDir, `${entity.name}Configuration.cs`);
    await this.writeRenderedTemplate('junction-configuration.generated.cs.hbs', context, filePath, true);
  }

  private getPropertyConfigurations(properties: Property[]): string[] {
    const configs: string[] = [];

    for (const prop of properties) {
      // Skip navigation properties and collections
      if (prop.isCollection || prop.navigationProperty) {
        continue;
      }

      const lines: string[] = [];

      // Start property configuration
      lines.push(`builder.Property(e => e.${prop.name})`);

      // Required
      if (prop.required) {
        lines.push('    .IsRequired()');
      }

      // MaxLength for strings
      if (prop.maxLength && TypeMapper.isStringType(prop.type)) {
        lines.push(`    .HasMaxLength(${prop.maxLength})`);
      }

      // Precision for decimal
      if (prop.type === 'decimal' && prop.precision) {
        const scale = prop.scale || 2;
        lines.push(`    .HasPrecision(${prop.precision}, ${scale})`);
      }

      // Default value
      if (prop.defaultValue !== undefined && prop.defaultValue !== null) {
        const defaultVal = TypeMapper.isStringType(prop.type)
          ? `"${prop.defaultValue}"`
          : prop.defaultValue.toString();
        lines.push(`    .HasDefaultValue(${defaultVal})`);
      }

      // Note: columnName property not in DataModel interface yet
      // if (prop.columnName && prop.columnName !== prop.name) {
      //   lines.push(`    .HasColumnName("${prop.columnName}")`);
      // }

      lines.push(';');
      configs.push(lines.join('\n'));
    }

    return configs;
  }

  private getIndexConfigurations(entity: Entity): string[] {
    const indexes: string[] = [];

    // Explicit indexes from entity definition
    if (entity.indexes) {
      for (const index of entity.indexes) {
        if (index.columns.length === 1) {
          // Single column index
          const indexDef = `builder.HasIndex(e => e.${index.columns[0]})`;
          if (index.isUnique) {
            indexes.push(`${indexDef}\n    .IsUnique();`);
          } else {
            indexes.push(`${indexDef};`);
          }
        } else {
          // Composite index
          const columns = index.columns.map((c) => `e.${c}`).join(', ');
          const indexDef = `builder.HasIndex(e => new { ${columns} })`;
          if (index.isUnique) {
            indexes.push(`${indexDef}\n    .IsUnique();`);
          } else {
            indexes.push(`${indexDef};`);
          }
        }
      }
    }

    // Common query pattern indexes (IsActive, CreatedAt, etc.)
    const commonIndexProps = ['IsActive', 'CreatedAt', 'UpdatedAt'];
    for (const propName of commonIndexProps) {
      const prop = entity.properties.find((p) => p.name === propName);
      if (prop && !entity.indexes?.some((idx) => idx.columns.includes(propName))) {
        indexes.push(`builder.HasIndex(e => e.${propName});`);
      }
    }

    return indexes;
  }

  private getRelationshipConfigurations(entity: Entity, model: DataModel): string[] {
    const relationships: string[] = [];

    // Find relationships where this entity is involved
    const entityRelationships = (model.relationships || []).filter(
      (r) =>
        r.leftEntity === entity.name ||
        r.rightEntity === entity.name ||
        (r.type === 'OneToMany' && r.rightEntity === entity.name)
    );

    for (const rel of entityRelationships) {
      if (rel.type === 'OneToMany' && rel.rightEntity === entity.name) {
        // This entity is the "many" side
        const fkProp = entity.properties.find(
          (p) => p.isForeignKey && p.referencedEntity === rel.leftEntity
        );

        if (fkProp && fkProp.navigationProperty) {
          const config = [
            `builder.HasOne(e => e.${fkProp.navigationProperty})`,
            `    .WithMany(p => p.${rel.leftNavigationProperty || this.pluralize(entity.name)})`,
            `    .HasForeignKey(e => e.${fkProp.name})`,
            `    .OnDelete(DeleteBehavior.Restrict);`,
          ].join('\n');
          relationships.push(config);
        }
      }
    }

    return relationships;
  }

  private pluralize(word: string): string {
    // Simple pluralization (extend as needed)
    if (word.endsWith('y')) {
      return word.slice(0, -1) + 'ies';
    } else if (word.endsWith('s') || word.endsWith('x') || word.endsWith('ch')) {
      return word + 'es';
    } else {
      return word + 's';
    }
  }
}
