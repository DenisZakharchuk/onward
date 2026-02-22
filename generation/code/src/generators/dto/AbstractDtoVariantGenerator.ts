import * as path from 'path';
import { BaseGenerator } from '../BaseGenerator';
import { BoundedContextGenerationContext, Entity, Property } from '../../models/DataModel';
import { TypeMapper } from '../../utils/TypeMapper';
import { IDTOLayoutGenerator } from './DtoVariantContracts';

export abstract class AbstractDtoVariantGenerator extends BaseGenerator implements IDTOLayoutGenerator {
  abstract readonly kind: 'class' | 'record';

  protected abstract getCreateTemplateName(): string | readonly string[];
  protected abstract getUpdateTemplateName(): string | readonly string[];
  protected abstract getDeleteTemplateName(): string | readonly string[];
  protected abstract getInitTemplateName(): string | readonly string[];
  protected abstract getDetailsTemplateName(): string | readonly string[];
  protected abstract getSearchTemplateName(): string | readonly string[];

  async generate(model: BoundedContextGenerationContext): Promise<void> {
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

    await this.generateCreateDto(entity, entityDir, namespace, baseNamespace);
    await this.generateUpdateDto(entity, entityDir, namespace, baseNamespace);
    await this.generateDeleteDto(entity, entityDir, namespace, baseNamespace);
    await this.generateInitDto(entity, entityDir, namespace, baseNamespace);
    await this.generateDetailsDto(entity, entityDir, namespace, baseNamespace);
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
      hasEnums: this.hasEnumProperties(entity, 'create'),
    };

    const filePath = path.join(entityDir, `Create${entity.name}DTO.cs`);
    await this.writeRenderedTemplate(this.getCreateTemplateName(), context, filePath, true);
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
      hasEnums: this.hasEnumProperties(entity, 'update'),
    };

    const filePath = path.join(entityDir, `Update${entity.name}DTO.cs`);
    await this.writeRenderedTemplate(this.getUpdateTemplateName(), context, filePath, true);
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
    await this.writeRenderedTemplate(this.getDeleteTemplateName(), context, filePath, true);
  }

  private async generateInitDto(
    entity: Entity,
    entityDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const properties = this.getInitDtoProperties(entity);

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      properties: properties.map((p) => ({
        name: p.name,
        type: TypeMapper.toCSharpType(p.enumType || p.type, false),
        description: p.description,
      })),
      hasEnums: properties.some((p) => p.enumType !== undefined),
    };

    const filePath = path.join(entityDir, `Init${entity.name}DTO.cs`);
    await this.writeRenderedTemplate(this.getInitTemplateName(), context, filePath, true);
  }

  private getInitDtoProperties(entity: Entity): Property[] {
    return entity.properties.filter((p) => {
      if (!p.required) {
        return false;
      }

      if (p.name === 'Id' || p.name === 'CreatedAt' || p.name === 'UpdatedAt') {
        return false;
      }

      if (p.isCollection) {
        return false;
      }

      if (p.navigationProperty && !p.isForeignKey) {
        return false;
      }

      if (p.includeInDto && p.includeInDto.create === false) {
        return false;
      }

      return true;
    });
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
      hasEnums: this.hasEnumProperties(entity, 'details'),
      navigationProperties: [],
      nestedDtos: [],
    };

    const filePath = path.join(entityDir, `${entity.name}DetailsDTO.cs`);
    await this.writeRenderedTemplate(this.getDetailsTemplateName(), context, filePath, true);
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
      properties: properties.map((p) => this.propertyToDto(p, 'search', true)),
      hasEnums: this.hasEnumProperties(entity, 'search'),
    };

    const filePath = path.join(entityDir, `${entity.name}SearchDTO.cs`);
    await this.writeRenderedTemplate(this.getSearchTemplateName(), context, filePath, true);
  }

  private getDtoProperties(
    entity: Entity,
    dtoType: 'create' | 'update' | 'details' | 'search'
  ): Property[] {
    return entity.properties.filter((p) => {
      if ((dtoType === 'create' || dtoType === 'update') && p.isCollection) {
        return false;
      }

      if ((dtoType === 'create' || dtoType === 'update') && p.navigationProperty && !p.isForeignKey) {
        return false;
      }

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
    if (dtoType !== 'create') {
      return null;
    }

    if (property.defaultValue) {
      return String(property.defaultValue);
    }

    if (property.type === 'string' && property.required) {
      return 'default!';
    }

    return null;
  }

  private hasEnumProperties(entity: Entity, dtoType: 'create' | 'update' | 'details' | 'search'): boolean {
    const properties = this.getDtoProperties(entity, dtoType);
    return properties.some((p) => p.enumType !== undefined);
  }
}