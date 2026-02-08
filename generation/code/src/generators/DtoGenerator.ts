/**
 * DTO generator - creates all 5 DTO types per entity
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property } from '../models/DataModel';
import { TypeMapper } from '../utils/TypeMapper';
import * as path from 'path';

export class DtoGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const dtoProjectPath = `${baseNamespace}.${contextName}.DTO`;

    for (const entity of model.entities) {
      await this.generateEntityDtos(entity, dtoProjectPath, namespace, baseNamespace);
    }
  }

  private async generateEntityDtos(
    entity: Entity,
    dtoProjectPath: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const entityDir = path.join(dtoProjectPath, 'DTO', entity.name);

    // Generate Create DTO
    await this.generateCreateDto(entity, entityDir, namespace, baseNamespace);

    // Generate Update DTO
    await this.generateUpdateDto(entity, entityDir, namespace, baseNamespace);

    // Generate Delete DTO
    await this.generateDeleteDto(entity, entityDir, namespace, baseNamespace);

    // Generate Details DTO
    await this.generateDetailsDto(entity, entityDir, namespace, baseNamespace);

    // Generate Search DTO
    await this.generateSearchDto(entity, entityDir, namespace, baseNamespace);
  }

  private async generateCreateDto(
    entity: Entity,
    entityDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const properties = this.getDtoProperties(entity, 'create');

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      properties: properties.map((p) => this.propertyToDto(p, 'create')),
    };

    const filePath = path.join(entityDir, `Create${entity.name}DTO.cs`);
    await this.writeRenderedTemplate('create-dto.cs.hbs', context, filePath, true);
  }

  private async generateUpdateDto(
    entity: Entity,
    entityDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const properties = this.getDtoProperties(entity, 'update');

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      properties: properties.map((p) => this.propertyToDto(p, 'update')),
    };

    const filePath = path.join(entityDir, `Update${entity.name}DTO.cs`);
    await this.writeRenderedTemplate('update-dto.cs.hbs', context, filePath, true);
  }

  private async generateDeleteDto(
    entity: Entity,
    entityDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
    };

    const filePath = path.join(entityDir, `Delete${entity.name}DTO.cs`);
    await this.writeRenderedTemplate('delete-dto.cs.hbs', context, filePath, true);
  }

  private async generateDetailsDto(
    entity: Entity,
    entityDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const properties = this.getDtoProperties(entity, 'details');

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      properties: properties.map((p) => this.propertyToDto(p, 'details')),
      navigationProperties: [],
      nestedDtos: [],
    };

    const filePath = path.join(entityDir, `${entity.name}DetailsDTO.cs`);
    await this.writeRenderedTemplate('details-dto.cs.hbs', context, filePath, true);
  }

  private async generateSearchDto(
    entity: Entity,
    entityDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const properties = this.getDtoProperties(entity, 'search');

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      properties: properties.map((p) => this.propertyToDto(p, 'search', true)), // All nullable for search
    };

    const filePath = path.join(entityDir, `${entity.name}SearchDTO.cs`);
    await this.writeRenderedTemplate('search-dto.cs.hbs', context, filePath, true);
  }

  private getDtoProperties(
    entity: Entity,
    dtoType: 'create' | 'update' | 'details' | 'search'
  ): Property[] {
    return entity.properties.filter((p) => {
      // Exclude collections and navigation properties from Create/Update
      if ((dtoType === 'create' || dtoType === 'update') && (p.isCollection || p.navigationProperty)) {
        return false;
      }

      // Check includeInDto settings
      if (p.includeInDto) {
        return p.includeInDto[dtoType] !== false;
      }

      return true;
    });
  }

  private propertyToDto(property: Property, dtoType: string, forceNullable: boolean = false): {
    name: string;
    type: string;
    description?: string;
    defaultValue: string | null;
    validationAttributes: string[];
  } {
    const isNullable = forceNullable || !property.required;
    const type = TypeMapper.toCSharpType(property.enumType || property.type, isNullable);

    return {
      name: property.name,
      type,
      description: property.description,
      defaultValue: this.getDefaultValue(property, dtoType),
      validationAttributes: dtoType !== 'details' ? TypeMapper.getValidationAttributes(property) : [],
    };
  }

  private getDefaultValue(property: Property, dtoType: string): string | null {
    // Only set defaults for Create DTOs
    if (dtoType !== 'create') {
      return null;
    }

    if (property.defaultValue) {
      return String(property.defaultValue);
    }

    // String properties default to default! for non-nullable reference types
    if (property.type === 'string' && property.required) {
      return 'default!';
    }

    return null;
  }
}
