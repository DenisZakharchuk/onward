/**
 * Service generator - creates DataService implementations
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity } from '../models/DataModel';
import * as path from 'path';

export class ServiceGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const servicesDir = `${baseNamespace}.${contextName}.BL/DataServices`;

    for (const entity of model.entities) {
      // Skip junction entities - they use RelationshipManagerBase instead
      if (entity.isJunction) {
        continue;
      }

      const ownershipCfg = model.boundedContext.ownership;
      const isContextOwned = ownershipCfg?.enabled === true;
      const ownershipValueObject = ownershipCfg?.valueObject ?? 'UserTenantOwnership';
      await this.generateDataService(entity, servicesDir, namespace, baseNamespace, isContextOwned, ownershipValueObject);
    }
  }

  private async generateDataService(
    entity: Entity,
    servicesDir: string,
    namespace: string,
    baseNamespace: string,
    isContextOwned: boolean = false,
    ownershipValueObject: string = 'UserTenantOwnership'
  ): Promise<void> {
    const isOwned = entity.owned === true && isContextOwned;
    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      isOwned,
      ownershipValueObject,
    };

    const filePath = path.join(servicesDir, `${entity.name}DataService.cs`);
    await this.writeRenderedTemplate(
      ['bl/data-service/generated.cs.hbs', 'data-service.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }
}
