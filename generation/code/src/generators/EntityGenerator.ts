/**
 * Entity generator - creates entity classes (both generated and custom stubs)
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property } from '../models/DataModel';
import { NamingConventions } from '../utils/NamingConventions';
import { TypeMapper } from '../utils/TypeMapper';
import { FileManager } from '../utils/FileManager';
import * as path from 'path';

export class EntityGenerator extends BaseGenerator {
  async generate(model: DataModel, outputDir: string): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;

    const domainProjectPath = path.join(outputDir, `Inventorization.${contextName}.Domain`);
    const entitiesDir = path.join(domainProjectPath, 'Entities');

    for (const entity of model.entities) {
      if (entity.isJunction) {
        await this.generateJunctionEntity(entity, entitiesDir, namespace);
      } else {
        await this.generateRegularEntity(entity, entitiesDir, namespace);
      }

      // Create custom stub if it doesn't exist
      await this.generateCustomEntityStub(entity, entitiesDir, namespace);
    }
  }

  private async generateRegularEntity(
    entity: Entity,
    entitiesDir: string,
    namespace: string
  ): Promise<void> {
    const properties = entity.properties.filter(
      (p) => !p.isCollection && !p.navigationProperty
    );

    const navigationProps = entity.properties.filter(
      (p) => p.isCollection || p.navigationProperty
    );

    const context = {
      namespace,
      entityName: entity.name,
      description: entity.description || entity.name,
      auditable: entity.auditable !== false,
      constructorParams: this.getConstructorParams(properties),
      validations: this.getValidations(properties),
      propertyAssignments: this.getPropertyAssignments(properties),
      properties: this.getProperties(properties),
      navigationProperties: this.getNavigationProperties(navigationProps),
    };

    const filePath = path.join(entitiesDir, `${entity.name}.generated.cs`);
    await this.writeRenderedTemplate('entity.generated.cs.hbs', context, filePath, true);
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

    const filePath = path.join(entitiesDir, `${entity.name}.generated.cs`);
    await this.writeRenderedTemplate('junction-entity.generated.cs.hbs', context, filePath, true);
  }

  private async generateCustomEntityStub(
    entity: Entity,
    entitiesDir: string,
    namespace: string
  ): Promise<void> {
    const filePath = path.join(entitiesDir, `${entity.name}.cs`);
    const exists = await FileManager.fileExists(filePath);

    if (exists) {
      return; // Don't overwrite custom logic
    }

    const updateableProps = entity.properties.filter(
      (p) => !p.isForeignKey && !p.isCollection && p.name !== 'Id'
    );

    const context = {
      namespace,
      entityName: entity.name,
      updateParams: updateableProps.map((p) => ({
        type: TypeMapper.toCSharpType(p.type, !p.required),
        paramName: NamingConventions.toCamelCase(p.name),
        paramDescription: p.description || p.name,
      })),
      updateAssignments: updateableProps.map(
        (p) =>
          `${p.name} = ${NamingConventions.toCamelCase(p.name)};`
      ),
    };

    await this.writeRenderedTemplate('entity.custom.cs.hbs', context, filePath, false);
  }

  private getConstructorParams(properties: Property[]): any[] {
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

  private getProperties(properties: Property[]): any[] {
    return properties.map((p) => ({
      name: p.name,
      type: TypeMapper.toCSharpType(p.type, !p.required),
      description: p.description,
      defaultValue: p.defaultValue || null,
    }));
  }

  private getNavigationProperties(properties: Property[]): any[] {
    return properties.map((p) => ({
      name: p.name,
      type: p.collectionType || p.referencedEntity || 'object',
      isCollection: p.isCollection,
      nullable: !p.required,
      description: p.description,
    }));
  }
}
