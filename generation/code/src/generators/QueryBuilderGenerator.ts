/**
 * QueryBuilderGenerator - generates ADT query builder classes
 */

import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext } from '../models/DataModel';
import * as path from 'path';

export class QueryBuilderGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const dataAccessDir = `${baseNamespace}.${contextName}.BL/DataAccess`;

    for (const entity of model.entities) {
      // Skip junction entities - they typically don't need query builders
      if (entity.isJunction) {
        continue;
      }

      const ownershipCfg = model.boundedContext.ownership;
      const isOwned = ownershipCfg?.enabled === true && entity.owned === true;
      const ownershipValueObject = ownershipCfg?.valueObject ?? 'UserTenantOwnership';

      const context = {
        namespace,
        baseNamespace,
        entityName: entity.name,
        description: entity.description || entity.name,
        isOwned,
        ownershipValueObject,
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
