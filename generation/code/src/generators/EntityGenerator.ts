/**
 * Entity generator - creates entity classes
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property, Relationship } from '../models/DataModel';
import { NamingConventions } from '../utils/NamingConventions';
import { TypeMapper } from '../utils/TypeMapper';
import * as path from 'path';

export class EntityGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const entitiesDir = `${baseNamespace}.${contextName}.Domain/Entities`;

    for (const entity of model.entities) {
      if (entity.isJunction) {
        await this.generateJunctionEntity(entity, entitiesDir, namespace);
      } else {
        await this.generateRegularEntity(entity, entitiesDir, namespace, model.relationships || []);
      }
    }
  }

  private async generateRegularEntity(
    entity: Entity,
    entitiesDir: string,
    namespace: string,
    relationships: Relationship[]
  ): Promise<void> {
    // Regular properties include FK properties (they are Guid properties)
    const properties = entity.properties.filter(
      (p) => !p.isCollection
    );

    // Generate navigation properties from FK metadata and relationships
    const navigationProps = this.buildNavigationProperties(entity.properties, entity.name, relationships);

    const context = {
      namespace,
      entityName: entity.name,
      description: entity.description || entity.name,
      auditable: entity.auditable !== false,
      constructorParams: this.getConstructorParams(properties),
      validations: this.getValidations(properties),
      propertyAssignments: this.getPropertyAssignments(properties),
      properties: this.getProperties(properties),
      navigationProperties: navigationProps,
    };

    const filePath = path.join(entitiesDir, `${entity.name}.cs`);
    await this.writeRenderedTemplate('entity.cs.hbs', context, filePath, true);
  }

  private async generateJunctionEntity(
    entity: Entity,
    entitiesDir: string,
    namespace: string
  ): Promise<void> {
    if (!entity.junctionMetadata) {
      throw new Error(`Junction entity ${entity.name} missing junctionMetadata`);
    }

    const metadataProps = entity.properties.filter(
      (p) => !p.isForeignKey && p.name !== 'Id'
    );

    const context = {
      namespace,
      junctionName: entity.name,
      leftEntity: entity.junctionMetadata.leftEntity,
      rightEntity: entity.junctionMetadata.rightEntity,
      leftEntityParam: NamingConventions.toCamelCase(entity.junctionMetadata.leftEntity),
      rightEntityParam: NamingConventions.toCamelCase(entity.junctionMetadata.rightEntity),
      hasMetadata: metadataProps.length > 0,
      metadataParams: metadataProps.map((p) => ({
        type: TypeMapper.toCSharpType(p.type, !p.required),
        paramName: NamingConventions.toCamelCase(p.name),
        paramDescription: p.description || p.name,
      })),
      metadataAssignments: metadataProps.map(
        (p) => `${p.name} = ${NamingConventions.toCamelCase(p.name)};`
      ),
      metadataProperties: metadataProps.map((p) => ({
        name: p.name,
        type: TypeMapper.toCSharpType(p.type, !p.required),
        description: p.description,
        defaultValue: null,
      })),
    };

    const filePath = path.join(entitiesDir, `${entity.name}.cs`);
    await this.writeRenderedTemplate('junction-entity.cs.hbs', context, filePath, true);
  }

  private getConstructorParams(properties: Property[]): Array<{
    type: string;
    paramName: string;
    paramDescription: string;
  }> {
    return properties
      .filter((p) => !p.isForeignKey || p.required)
      .map((p) => ({
        type: TypeMapper.toCSharpType(p.type, !p.required),
        paramName: NamingConventions.toCamelCase(p.name),
        paramDescription: p.description || p.name,
      }));
  }

  private getValidations(properties: Property[]): string[] {
    const validations: string[] = [];

    for (const prop of properties) {
      const paramName = NamingConventions.toCamelCase(prop.name);

      if (prop.required && TypeMapper.isStringType(prop.type)) {
        validations.push(
          `if (string.IsNullOrWhiteSpace(${paramName})) throw new ArgumentException("${prop.name} cannot be empty", nameof(${paramName}));`
        );
      }

      if (prop.maxLength && TypeMapper.isStringType(prop.type)) {
        validations.push(
          `if (${paramName}.Length > ${prop.maxLength}) throw new ArgumentException("${prop.name} cannot exceed ${prop.maxLength} characters", nameof(${paramName}));`
        );
      }

      if (prop.validation?.min !== undefined && TypeMapper.isNumericType(prop.type)) {
        validations.push(
          `if (${paramName} < ${prop.validation.min}) throw new ArgumentException("${prop.name} must be at least ${prop.validation.min}", nameof(${paramName}));`
        );
      }
    }

    return validations;
  }

  private getPropertyAssignments(properties: Property[]): string[] {
    return properties.map(
      (p) => `${p.name} = ${NamingConventions.toCamelCase(p.name)};`
    );
  }

  private getProperties(properties: Property[]): Array<{
    name: string;
    type: string;
    description?: string;
    defaultValue: unknown;
  }> {
    return properties.map((p) => ({
      name: p.name,
      type: TypeMapper.toCSharpType(p.type, !p.required),
      description: p.description,
      defaultValue: p.defaultValue || null,
    }));
  }

  /**
   * Build navigation properties from FK metadata and relationships
   */
  private buildNavigationProperties(
    properties: Property[],
    entityName: string,
    relationships: Relationship[]
  ): Array<{
    name: string;
    type: string;
    isCollection?: boolean;
    nullable: boolean;
    description?: string;
  }> {
    const navProps: Array<{
      name: string;
      type: string;
      isCollection?: boolean;
      nullable: boolean;
      description?: string;
    }> = [];

    // 1. Single navigation properties from FK properties
    for (const prop of properties) {
      if (prop.isForeignKey && prop.navigationProperty && prop.referencedEntity) {
        navProps.push({
          name: prop.navigationProperty,
          type: prop.referencedEntity,
          isCollection: false,
          nullable: !prop.required,
          description: `Navigation to ${prop.referencedEntity}`,
        });
      }
      // Collection properties defined directly in entity
      else if (prop.isCollection && prop.collectionType) {
        navProps.push({
          name: prop.name,
          type: prop.collectionType,
          isCollection: true,
          nullable: false,
          description: prop.description,
        });
      }
    }

    // 2. Collection navigation properties from relationships
    for (const rel of relationships) {
      // One-to-Many: leftEntity has collection of rightEntity
      if (rel.type === 'OneToMany' && rel.leftEntity === entityName && rel.leftNavigationProperty) {
        navProps.push({
          name: rel.leftNavigationProperty,
          type: rel.rightEntity,
          isCollection: true,
          nullable: false,
          description: `Collection of ${rel.rightEntity}`,
        });
      }
      // Many-to-Many: both sides get collections (handled via junction entities)
      // Many-to-One: rightEntity has single navigation (already handled by FK above)
    }

    return navProps;
  }
}
