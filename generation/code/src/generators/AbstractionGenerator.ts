/**
 * Abstraction generator - creates Creators, Modifiers, Mappers, and SearchProviders
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity } from '../models/DataModel';
import { TypeMapper } from '../utils/TypeMapper';
import { NamingConventions } from '../utils/NamingConventions';
import * as path from 'path';

export class AbstractionGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const domainProjectPath = `${baseNamespace}.${contextName}.Domain`;

    for (const entity of model.entities) {
      // Skip junction entities - they don't need full CRUD abstractions
      if (entity.isJunction) {
        continue;
      }

      await this.generateCreator(entity, domainProjectPath, namespace, baseNamespace);
      await this.generateModifier(entity, domainProjectPath, namespace, baseNamespace);
      await this.generateMapper(entity, domainProjectPath, namespace, baseNamespace);
      await this.generateSearchProvider(entity, domainProjectPath, namespace, baseNamespace);
    }
  }

  private async generateCreator(
    entity: Entity,
    domainProjectPath: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const creatorsDir = path.join(domainProjectPath, 'Creators');
    
    // Get constructor arguments from Create DTO
    const constructorArgs = this.getConstructorArgs(entity);

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      hasDependencies: false,
      dependencies: [],
      transformations: [],
      constructorArgs,
    };

    const filePath = path.join(creatorsDir, `${entity.name}Creator.cs`);
    await this.writeRenderedTemplate('creator.generated.cs.hbs', context, filePath, true);
  }

  private async generateModifier(
    entity: Entity,
    domainProjectPath: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const modifiersDir = path.join(domainProjectPath, 'Modifiers');

    // Get properties that can be modified (exclude Id, CreatedAt, UpdatedAt, navigation props, FKs)
    const modifiableProps = entity.properties.filter(
      (p) =>
        p.name !== 'Id' &&
        p.name !== 'CreatedAt' &&
        p.name !== 'UpdatedAt' &&
        !p.isCollection &&
        !p.navigationProperty &&
        !p.isForeignKey
    );

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      hasDependencies: false,
      dependencies: [],
      updateArgs: modifiableProps.map((p) => ({
        argName: NamingConventions.toCamelCase(p.name),
        argValue: `dto.${p.name}`,
      })),
      conditionalUpdates: [],
    };

    const filePath = path.join(modifiersDir, `${entity.name}Modifier.cs`);
    await this.writeRenderedTemplate('modifier.generated.cs.hbs', context, filePath, true);
  }

  private async generateMapper(
    entity: Entity,
    domainProjectPath: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const mappersDir = path.join(domainProjectPath, 'Mappers');

    // Get all properties that should be mapped to DetailsDTO
    const mappableProps = entity.properties.filter(
      (p) => !p.isCollection && !p.navigationProperty
    );

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      propertyMappings: mappableProps.map(
        (p) => ({
          dtoProperty: p.name,
          entityProperty: `entity.${p.name}`,
        })
      ),
    };

    const filePath = path.join(mappersDir, `${entity.name}Mapper.cs`);
    await this.writeRenderedTemplate('mapper.generated.cs.hbs', context, filePath, true);
  }

  private async generateSearchProvider(
    entity: Entity,
    domainProjectPath: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const searchProvidersDir = path.join(domainProjectPath, 'SearchProviders');

    // Identify searchable properties (text fields, dates, status fields)
    const searchableProps = entity.properties.filter(
      (p) =>
        TypeMapper.isStringType(p.type) ||
        p.type === 'DateTime' ||
        p.name.toLowerCase().includes('status') ||
        p.name === 'IsActive'
    );

    // Build search conditions from searchable properties
    const searchConditions = searchableProps.map((p) => {
      const propName = p.name;
      const isString = TypeMapper.isStringType(p.type);
      const isDateTime = p.type === 'DateTime';
      const isBoolean = p.type === 'bool';

      if (isString) {
        return { condition: `(searchDto.${propName} == null || entity.${propName}.Contains(searchDto.${propName}))` };
      } else if (isDateTime) {
        return { condition: `(searchDto.${propName} == null || entity.${propName} == searchDto.${propName})` };
      } else if (isBoolean) {
        return { condition: `(searchDto.${propName} == null || entity.${propName} == searchDto.${propName})` };
      } else {
        return { condition: `(searchDto.${propName} == null || entity.${propName} == searchDto.${propName})` };
      }
    });

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      searchConditions: searchConditions.length > 0 ? searchConditions : [{ condition: 'true' }],
    };

    const filePath = path.join(searchProvidersDir, `${entity.name}SearchProvider.cs`);
    await this.writeRenderedTemplate('search-provider.generated.cs.hbs', context, filePath, true);
  }

  private getConstructorArgs(entity: Entity): Array<{
    argName: string;
    argValue: string;
  }> {
    // Get properties that go into entity constructor
    // This filter must match EntityGenerator.getConstructorParams logic:
    // Include all non-FK properties + required FK properties
    // Exclude: Id, CreatedAt, UpdatedAt, and collection navigation properties
    const constructorProps = entity.properties.filter(
      (p) =>
        p.name !== 'Id' &&
        p.name !== 'CreatedAt' &&
        p.name !== 'UpdatedAt' &&
        !p.isCollection &&
        (!p.isForeignKey || p.required)
    );

    return constructorProps.map((p) => ({
      argName: NamingConventions.toCamelCase(p.name),
      argValue: `dto.${p.name}`,
    }));
  }
}
