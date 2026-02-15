/**
 * ProjectionDtoGenerator - generates projection DTO classes
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property, Relationship } from '../models/DataModel';
import { TypeMapper } from '../utils/TypeMapper';
import { NamingConventions } from '../utils/NamingConventions';
import * as path from 'path';

export class ProjectionDtoGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const adtsDir = `${baseNamespace}.${contextName}.DTO/ADTs`;

    for (const entity of model.entities) {
      // Skip junction entities
      if (entity.isJunction) {
        continue;
      }

      await this.generateProjectionDto(
        entity,
        adtsDir,
        namespace,
        model.relationships || []
      );
    }
  }

  private async generateProjectionDto(
    entity: Entity,
    adtsDir: string,
    namespace: string,
    relationships: Relationship[]
  ): Promise<void> {
    // Get all scalar properties
    const scalarProperties = entity.properties.filter(
      (p) => !p.isCollection && (!p.isForeignKey || !p.navigationProperty)
    );

    // Get related entities for nested projections
    const relatedEntities = this.getRelatedEntities(entity, relationships);

    // Build base properties from BaseEntity (Id, CreatedAt, UpdatedAt)
    const baseProperties = [
      {
        name: 'Id',
        type: 'Guid?',
        jsonPropertyName: 'id',
        description: 'Unique identifier',
      },
      {
        name: 'CreatedAt',
        type: 'DateTime?',
        jsonPropertyName: 'createdAt',
        description: 'Created timestamp',
      },
    ];

    // Add UpdatedAt only if entity is auditable
    if (entity.auditable !== false) {
      baseProperties.push({
        name: 'UpdatedAt',
        type: 'DateTime?',
        jsonPropertyName: 'updatedAt',
        description: 'Last updated timestamp',
      });
    }

    // Combine base properties with entity properties
    const allProperties = [
      ...baseProperties,
      ...this.buildPropertyContext(scalarProperties),
    ];

    // Check if entity has enum properties
    const hasEnums = scalarProperties.some((p) => p.enumType !== undefined);

    const context = {
      namespace,
      entityName: entity.name,
      projectionName: `${entity.name}Projection`,
      description: entity.description || entity.name,
      properties: allProperties,
      relationships: this.buildRelationshipContext(relatedEntities),
      hasRelationships: relatedEntities.length > 0,
      hasEnums,
    };

    const filePath = path.join(adtsDir, `${entity.name}Projection.cs`);
    await this.writeRenderedTemplate(
      ['dto/projection.generated.cs.hbs', 'projection-dto.generated.cs.hbs'],
      context,
      filePath,
      true // Overwrite allowed
    );
  }

  private getRelatedEntities(
    entity: Entity,
    _relationships: Relationship[]
  ): Array<{ name: string; targetEntity: string; nullable: boolean }> {
    const related: Array<{ name: string; targetEntity: string; nullable: boolean }> = [];

    // From FK properties with navigation
    for (const prop of entity.properties) {
      if (prop.isForeignKey && prop.navigationProperty && prop.referencedEntity) {
        related.push({
          name: prop.navigationProperty,
          targetEntity: prop.referencedEntity,
          nullable: true, // All related entities nullable in projections
        });
      }
    }

    return related;
  }

  private buildPropertyContext(properties: Property[]): Array<{
    name: string;
    type: string;
    jsonPropertyName: string;
    description: string;
  }> {
    return properties.map((p) => ({
      name: p.name,
      type: TypeMapper.toCSharpType(p.enumType || p.type, true), // Handle enums, always nullable
      jsonPropertyName: NamingConventions.toCamelCase(p.name),
      description: p.description || p.name,
    }));
  }

  private buildRelationshipContext(
    relatedEntities: Array<{ name: string; targetEntity: string; nullable: boolean }>
  ): Array<{
    name: string;
    targetProjection: string;
    jsonPropertyName: string;
  }> {
    return relatedEntities.map((r) => ({
      name: r.name,
      targetProjection: `${r.targetEntity}Projection`,
      jsonPropertyName: NamingConventions.toCamelCase(r.name),
    }));
  }
}
