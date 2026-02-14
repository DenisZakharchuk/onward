/**
 * ProjectionMapperInterfaceGenerator - generates projection mapper interface
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel } from '../models/DataModel';
import * as path from 'path';

export class ProjectionMapperInterfaceGenerator extends BaseGenerator {
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

      const context = {
        namespace,
        baseNamespace,
        entityName: entity.name,
        projectionName: `${entity.name}Projection`,
      };

      const filePath = path.join(mappersDir, `I${entity.name}ProjectionMapper.cs`);
      await this.writeRenderedTemplate(
        'projection-mapper-interface.generated.cs.hbs',
        context,
        filePath,
        true // Overwrite allowed
      );
    }
  }
}
