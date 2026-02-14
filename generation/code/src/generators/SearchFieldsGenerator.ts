/**
 * SearchFieldsGenerator - generates search field constants
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Relationship } from '../models/DataModel';
import * as path from 'path';

export class SearchFieldsGenerator extends BaseGenerator {
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

      await this.generateSearchFields(
        entity,
        adtsDir,
        namespace,
        model.relationships || []
      );
    }
  }

  private async generateSearchFields(
    entity: Entity,
    adtsDir: string,
    namespace: string,
    relationships: Relationship[]
  ): Promise<void> {
    // Get direct properties
    const directProperties = entity.properties.filter(
      (p) => !p.isCollection
    );

    // Get nested field paths for related entities
    const nestedFields = this.getNestedFields(entity, relationships);

    const context = {
      namespace,
      entityName: entity.name,
      description: entity.description || entity.name,
      directFields: directProperties.map((p) => ({
        constantName: p.name,
        value: p.name,
      })),
      nestedFields,
      allFieldNames: [
        ...directProperties.map((p) => p.name),
        ...nestedFields.map((f) => f.value),
      ],
    };

    const filePath = path.join(adtsDir, `${entity.name}SearchFields.cs`);
    await this.writeRenderedTemplate(
      'search-fields.generated.cs.hbs',
      context,
      filePath,
      true // Overwrite allowed
    );
  }

  private getNestedFields(
    entity: Entity,
    _relationships: Relationship[]
  ): Array<{ constantName: string; value: string }> {
    const nested: Array<{ constantName: string; value: string }> = [];

    // From FK properties with navigation
    for (const prop of entity.properties) {
      if (prop.isForeignKey && prop.navigationProperty && prop.referencedEntity) {
        // Add common nested fields (Id, Name are typically available)
        nested.push({
          constantName: `${prop.navigationProperty}Id`,
          value: `${prop.navigationProperty}.Id`,
        });
        nested.push({
          constantName: `${prop.navigationProperty}Name`,
          value: `${prop.navigationProperty}.Name`,
        });
      }
    }

    return nested;
  }
}
