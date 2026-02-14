/**
 * SearchServiceGenerator - generates ADT search service classes
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel } from '../models/DataModel';
import * as path from 'path';

export class SearchServiceGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const servicesDir = `${baseNamespace}.${contextName}.Domain/Services`;

    for (const entity of model.entities) {
      // Skip junction entities
      if (entity.isJunction) {
        continue;
      }

      const context = {
        namespace,
        entityName: entity.name,
        projectionName: `${entity.name}Projection`,
        description: entity.description || entity.name,
      };

      const filePath = path.join(servicesDir, `${entity.name}SearchService.cs`);
      await this.writeRenderedTemplate(
        'search-service.generated.cs.hbs',
        context,
        filePath,
        true // Overwrite allowed
      );
    }
  }
}
