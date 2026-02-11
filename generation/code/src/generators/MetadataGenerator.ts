/**
 * Metadata generator - creates DataModelMetadata and DataModelRelationships classes
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property, Relationship } from '../models/DataModel';
import { TypeMapper } from '../utils/TypeMapper';
import * as path from 'path';

export class MetadataGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const metaProjectPath = `${baseNamespace}.${contextName}.Meta`;

    await this.generateDataModelMetadata(model, metaProjectPath, namespace, baseNamespace, contextName);
    await this.generateDataModelRelationships(model, metaProjectPath, namespace, baseNamespace, contextName);
  }

  private async generateDataModelMetadata(
    model: DataModel,
    metaProjectPath: string,
    namespace: string,
    baseNamespace: string,
    contextName: string
  ): Promise<void> {
    const entities = model.entities
      .filter((e) => !e.isJunction)
      .map((entity) => this.buildEntityMetadataContext(entity, model));

    const context = {
      baseNamespace,
      namespace,
      contextName,
      entities,
    };

    const filePath = path.join(metaProjectPath, 'DataModelMetadata.cs');
    await this.writeRenderedTemplate('data-model-metadata.generated.cs.hbs', context, filePath, true);
  }

  private async generateDataModelRelationships(
    model: DataModel,
    metaProjectPath: string,
    namespace: string,
    baseNamespace: string,
    contextName: string
  ): Promise<void> {
    const relationships = this.buildRelationshipMetadataContexts(model);

    const context = {
      baseNamespace,
      namespace,
      contextName,
      relationships,
    };

    const filePath = path.join(metaProjectPath, 'DataModelRelationships.cs');
    await this.writeRenderedTemplate('data-model-relationships.generated.cs.hbs', context, filePath, true);
  }

  private buildEntityMetadataContext(entity: Entity, model: DataModel): Record<string, unknown> {
    // Find all relationships involving this entity
    const entityRelationships = (model.relationships || []).filter(
      (r) => r.leftEntity === entity.name || r.rightEntity === entity.name
    );

    const relationshipRefs = entityRelationships.map((r) => this.getRelationshipName(r));

    return {
      name: entity.name,
      tableName: entity.tableName || this.pluralize(entity.name),
      schema: entity.schema,
      displayName: entity.name,
      description: entity.description || `${entity.name} entity`,
      auditable: entity.auditable !== false,
      isJunction: entity.isJunction || false,
      properties: entity.properties.map((p) => this.buildPropertyMetadataContext(p, entity.name)),
      indexes: this.getIndexedProperties(entity), // For old template usage
      indexMetadata: this.buildIndexMetadata(entity), // For new template
      uniqueConstraints: this.buildUniqueConstraintMetadata(entity), // For new template
      hasRelationships: relationshipRefs.length > 0,
      relationshipRefs,
    };
  }

  private buildPropertyMetadataContext(prop: Property, _entityName: string): Record<string, unknown> {
    const isRequired = prop.required ?? true;
    const isNullable = !isRequired;
    
    return {
      name: prop.name,
      type: TypeMapper.toCSharpType(prop.type as string, isNullable),
      typeFullName: this.getCSharpTypeFullName(prop),
      nullable: isNullable,
      displayName: prop.displayName || prop.name,
      description: prop.description || `${prop.name} property`,
      isPrimaryKey: prop.name === 'Id',
      required: isRequired,
      maxLength: prop.maxLength,
      minLength: prop.minLength,
      precision: prop.precision,
      scale: prop.scale,
      columnType: this.getColumnType(prop),
      isUnique: this.isUniqueProperty(prop),
      isForeignKey: prop.isForeignKey || false,
      isIndexed: prop.isForeignKey || this.isIndexedProperty(prop),
      minValue: prop.validation?.min,
      maxValue: prop.validation?.max,
      regexPattern: prop.validation?.regex,
      isEmail: prop.validation?.regex === '.+@.+\\..+',
      defaultValue: this.formatDefaultValue(prop),
      defaultValueSql: this.getDefaultValueSql(prop),
      validationMessage: prop.validationMessage,
    };
  }

  private buildRelationshipMetadataContexts(model: DataModel): Array<Record<string, unknown>> {
    const relationships: Array<Record<string, unknown>> = [];

    // Junction entity relationships (ManyToMany)
    for (const entity of model.entities.filter((e) => e.isJunction && e.junctionMetadata)) {
      const junctionMeta = entity.junctionMetadata!;
      relationships.push({
        name: entity.name,
        type: 'ManyToMany',
        typeDescription: 'many-to-many relationship via ' + entity.name + ' junction',
        leftEntity: junctionMeta.leftEntity,
        rightEntity: junctionMeta.rightEntity,
        junctionEntityName: entity.name,
        navigationPropertyName: entity.name + 's',
        displayName: `${junctionMeta.leftEntity} ${junctionMeta.rightEntity}`,
        cardinality: 'Optional',
        description: entity.description || `${junctionMeta.leftEntity} to ${junctionMeta.rightEntity} relationship`,
      });
    }

    // Explicit relationships from model.relationships
    for (const rel of model.relationships || []) {
      if (rel.type === 'ManyToMany') {
        // Already handled via junction entities
        continue;
      }

      relationships.push({
        name: this.getRelationshipName(rel),
        type: rel.type,
        typeDescription: this.getRelationshipTypeDescription(rel.type),
        leftEntity: rel.leftEntity,
        rightEntity: rel.rightEntity,
        junctionEntityName: rel.junctionEntity,
        navigationPropertyName: rel.leftNavigationProperty || this.pluralize(rel.rightEntity),
        inverseNavigationPropertyName: rel.rightNavigationProperty,
        foreignKeyPropertyName: rel.foreignKeyProperty,
        displayName: rel.displayName || `${rel.leftEntity} ${rel.rightEntity}`,
        cardinality: rel.cardinality || 'Optional',
        description: rel.description || `${rel.leftEntity} to ${rel.rightEntity} relationship`,
      });
    }

    return relationships;
  }

  private getRelationshipName(rel: Relationship): string {
    if (rel.junctionEntity) {
      return rel.junctionEntity;
    }
    if (rel.leftNavigationProperty) {
      return `${rel.leftEntity}${rel.leftNavigationProperty}`;
    }
    return `${rel.leftEntity}${this.pluralize(rel.rightEntity)}`;
  }

  private getRelationshipTypeDescription(type: string): string {
    switch (type) {
      case 'OneToOne':
        return 'one-to-one relationship';
      case 'OneToMany':
        return 'one-to-many relationship';
      case 'ManyToOne':
        return 'many-to-one relationship';
      case 'ManyToMany':
        return 'many-to-many relationship';
      default:
        return 'relationship';
    }
  }

  private getCSharpTypeFullName(prop: Property): string {
    const baseType = TypeMapper.toCSharpType(prop.type, false);
    return prop.required ? baseType : `${baseType}?`;
  }

  private getColumnType(prop: Property): string | undefined {
    if (prop.type === 'decimal' && prop.precision) {
      const scale = prop.scale || 2;
      return `decimal(${prop.precision},${scale})`;
    }
    return undefined;
  }

  private isUniqueProperty(prop: Property): boolean {
    return prop.name === 'SKU' || prop.name === 'Code' || prop.name === 'Email';
  }

  private isIndexedProperty(prop: Property): boolean {
    return (
      prop.name === 'IsActive' ||
      prop.name === 'CreatedAt' ||
      prop.name === 'UpdatedAt' ||
      prop.isForeignKey === true
    );
  }

  private getIndexedProperties(entity: Entity): string[] {
    const indexed: string[] = [];

    // From explicit indexes
    if (entity.indexes) {
      for (const index of entity.indexes) {
        indexed.push(...index.columns);
      }
    }

    // Common indexed properties
    for (const prop of entity.properties) {
      if (this.isIndexedProperty(prop) && !indexed.includes(prop.name)) {
        indexed.push(prop.name);
      }
    }

    return [...new Set(indexed)]; // Remove duplicates
  }

  // Kept for potential future use
  // private getUniqueProperties(entity: Entity): string[] {
  //   const unique: string[] = [];

  //   // From explicit indexes
  //   if (entity.indexes) {
  //     for (const index of entity.indexes) {
  //       if (index.isUnique && index.columns.length === 1) {
  //         unique.push(index.columns[0]);
  //       }
  //     }
  //   }

  //   // Properties known to be unique
  //   for (const prop of entity.properties) {
  //     if (this.isUniqueProperty(prop) && !unique.includes(prop.name)) {
  //       unique.push(prop.name);
  //     }
  //   }

  //   return unique;
  // }

  private buildIndexMetadata(entity: Entity): Array<Record<string, unknown>> {
    const indexMetadata: Array<Record<string, unknown>> = [];

    // Explicit indexes from entity definition
    if (entity.indexes) {
      for (const index of entity.indexes) {
        indexMetadata.push({
          name: index.name || `IX_${entity.tableName || entity.name}_${index.columns.join('_')}`,
          columns: index.columns,
          isUnique: index.isUnique || false,
        });
      }
    }

    // Auto-generate indexes for foreign keys
    for (const prop of entity.properties) {
      if (this.isIndexedProperty(prop) && !entity.indexes?.some(idx => idx.columns.includes(prop.name))) {
        indexMetadata.push({
          name: `IX_${entity.tableName || entity.name}_${prop.name}`,
          columns: [prop.name],
          isUnique: false,
        });
      }
    }

    return indexMetadata;
  }

  private buildUniqueConstraintMetadata(entity: Entity): Array<Record<string, unknown>> {
    const uniqueConstraints: Array<Record<string, unknown>> = [];

    // Unique constraints from indexes
    if (entity.indexes) {
      for (const index of entity.indexes) {
        if (index.isUnique) {
          uniqueConstraints.push({
            name: index.name || `UQ_${entity.tableName || entity.name}_${index.columns.join('_')}`,
            columns: index.columns,
          });
        }
      }
    }

    return uniqueConstraints;
  }

  private formatDefaultValue(prop: Property): string | undefined {
    if (prop.defaultValue === undefined || prop.defaultValue === null) {
      return undefined;
    }

    if (TypeMapper.isStringType(prop.type)) {
      return `"${prop.defaultValue}"`;
    }

    if (prop.type === 'bool') {
      return prop.defaultValue.toString().toLowerCase();
    }

    if (prop.type === 'decimal') {
      return `${prop.defaultValue}m`;
    }

    return prop.defaultValue.toString();
  }

  private getDefaultValueSql(prop: Property): string | undefined {
    if (prop.name === 'CreatedAt' || prop.name === 'UpdatedAt') {
      return 'GETUTCDATE()';
    }
    return undefined;
  }

  private pluralize(word: string): string {
    if (word.endsWith('y')) {
      return word.slice(0, -1) + 'ies';
    } else if (word.endsWith('s') || word.endsWith('x') || word.endsWith('ch')) {
      return word + 'es';
    } else {
      return word + 's';
    }
  }
}
