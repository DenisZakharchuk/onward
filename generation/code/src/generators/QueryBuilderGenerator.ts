/**
 * QueryBuilderGenerator - generates ADT query builder classes
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel } from '../models/DataModel';
import * as path from 'path';

export class QueryBuilderGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const dataAccessDir = `${baseNamespace}.${contextName}.Domain/DataAccess`;

    for (const entity of model.entities) {
      // Skip junction entities - they typically don't need query builders
      if (entity.isJunction) {
        continue;
      }

      const context = {
        namespace,
        baseNamespace,
        entityName: entity.name,
        description: entity.description || entity.name,
      };

      const filePath = path.join(dataAccessDir, `${entity.name}QueryBuilder.cs`);
      await this.writeRenderedTemplate(
        ['query/query-builder.generated.cs.hbs', 'query-builder.generated.cs.hbs'],
        context,
        filePath,
        true // Overwrite allowed
      );
    }
  }
}
