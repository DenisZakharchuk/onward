/**
 * Controller generator - creates API controllers
 */

import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext, Entity } from '../models/DataModel';
import * as path from 'path';

export class ControllerGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const controllersDir = `${baseNamespace}.${contextName}.API/Controllers`;

    const ownershipCfg = model.boundedContext.ownership;
    const isContextOwned = ownershipCfg?.enabled === true;
    const ownershipValueObject = ownershipCfg?.valueObject ?? 'UserTenantOwnership';

    for (const entity of model.entities) {
      // Skip junction entities - they use relationship endpoints on parent entities
      if (entity.isJunction) {
        continue;
      }

      await this.generateController(entity, controllersDir, namespace, baseNamespace, isContextOwned, ownershipValueObject);
    }
  }

  private async generateController(
    entity: Entity,
    controllersDir: string,
    namespace: string,
    baseNamespace: string,
    isContextOwned: boolean = false,
    ownershipValueObject: string = 'UserTenantOwnership'
  ): Promise<void> {
    const pluralEntityName = this.pluralize(entity.name);
    const isOwned = entity.owned === true && isContextOwned;

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      pluralEntityName,
      isOwned,
      ownershipValueObject,
    };

    const filePath = path.join(controllersDir, `${pluralEntityName}Controller.cs`);
    await this.writeRenderedTemplate(
      ['api/controllers/crud.generated.cs.hbs', 'controller.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }

  private pluralize(word: string): string {
    // Simple pluralization
    if (word.endsWith('y')) {
      return word.slice(0, -1) + 'ies';
    } else if (word.endsWith('s') || word.endsWith('x') || word.endsWith('ch')) {
      return word + 'es';
    } else {
      return word + 's';
    }
  }
}
