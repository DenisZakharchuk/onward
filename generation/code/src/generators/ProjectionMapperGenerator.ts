/**
 * ProjectionMapperGenerator - generates projection mapper implementations
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property, Relationship } from '../models/DataModel';
import { TypeMapper } from '../utils/TypeMapper';
import { NamingConventions } from '../utils/NamingConventions';
import * as path from 'path';

export class ProjectionMapperGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const mappersDir = `${baseNamespace}.${contextName}.Domain/Mappers/Projection`;

    for (const entity of model.entities) {
      // Skip junction entities
      if (entity.isJunction) {
        continue;
      }

      await this.generateProjectionMapper(
        entity,
        mappersDir,
        namespace,
        model.relationships || []
      );
    }
  }

  private async generateProjectionMapper(
    entity: Entity,
    mappersDir: string,
    namespace: string,
    relationships: Relationship[]
  ): Promise<void> {
    // Get scalar properties (exclude collections and navigation properties)
    const scalarProperties = entity.properties.filter(
      (p) => !p.isCollection && (!p.isForeignKey || !p.navigationProperty)
    );

    // Get related entities through FK properties or relationships
    const relatedEntities = this.getRelatedEntities(entity, relationships);

    const context = {
      namespace,
      entityName: entity.name,
      projectionName: `${entity.name}Projection`,
      properties: this.buildPropertyContext(scalarProperties),
      relationships: this.buildRelationshipContext(relatedEntities, entity.name),
      hasRelationships: relatedEntities.length > 0,
      maxDefaultDepth: 3, // Default depth for nested projections
    };

    const filePath = path.join(mappersDir, `${entity.name}ProjectionMapper.cs`);
    await this.writeRenderedTemplate(
      'projection-mapper.generated.cs.hbs',
      context,
      filePath,
      true // Overwrite allowed
    );
  }

  private getRelatedEntities(
    entity: Entity,
    relationships: Relationship[]
  ): Array<{ name: string; targetEntity: string; isCollection: boolean; nullable: boolean }> {
    const related: Array<{ name: string; targetEntity: string; isCollection: boolean; nullable: boolean }> =
      [];

    // From FK properties with navigation
    for (const prop of entity.properties) {
      if (prop.isForeignKey && prop.navigationProperty && prop.referencedEntity) {
        related.push({
          name: prop.navigationProperty,
          targetEntity: prop.referencedEntity,
          isCollection: false,
          nullable: !prop.required,
        });
      }
    }

    // From relationships (collections)
    for (const rel of relationships) {
      if (rel.type === 'OneToMany' && rel.leftEntity === entity.name && rel.leftNavigationProperty) {
        // Don't include collection navigations in projections by default
        // Collections usually handled separately or not projected
        // Uncomment if needed:
        // related.push({
        //   name: rel.leftNavigationProperty,
        //   targetEntity: rel.rightEntity,
        //   isCollection: true,
        //   nullable: false,
        // });
      }
      if (rel.type === 'ManyToMany' && (rel.leftEntity === entity.name || rel.rightEntity === entity.name)) {
        // Skip many-to-many for now
      }
    }

    return related;
  }

  private buildPropertyContext(properties: Property[]): Array<{
    name: string;
    type: string;
    typeNullable: string;
    isNullable: boolean;
    camelName: string;
    pascalName: string;
  }> {
    return properties.map((p) => ({
      name: p.name,
      type: TypeMapper.toCSharpType(p.type, false),
      typeNullable: TypeMapper.toCSharpType(p.type, true),
      isNullable: !p.required,
      camelName: NamingConventions.toCamelCase(p.name),
      pascalName: p.name,
    }));
  }

  private buildRelationshipContext(
    relatedEntities: Array<{ name: string; targetEntity: string; isCollection: boolean; nullable: boolean }>,
    _currentEntityName: string
  ): Array<{
    name: string;
    targetEntity: string;
    targetProjection: string;
    targetMapperInterface: string;
    isCollection: boolean;
    nullable: boolean;
    camelName: string;
    camelTargetEntity: string;
  }> {
    return relatedEntities.map((r) => ({
      name: r.name,
      targetEntity: r.targetEntity,
      targetProjection: `${r.targetEntity}Projection`,
      targetMapperInterface: `I${r.targetEntity}ProjectionMapper`,
      isCollection: r.isCollection,
      nullable: r.nullable,
      camelName: NamingConventions.toCamelCase(r.name),
      camelTargetEntity: NamingConventions.toCamelCase(r.targetEntity),
    }));
  }
}
