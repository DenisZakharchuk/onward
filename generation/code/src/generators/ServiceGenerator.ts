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

    const servicesDir = `${baseNamespace}.${contextName}.Domain/DataServices`;

    for (const entity of model.entities) {
      // Skip junction entities - they use RelationshipManagerBase instead
      if (entity.isJunction) {
        continue;
      }

      await this.generateDataService(entity, servicesDir, namespace, baseNamespace);
    }
  }

  private async generateDataService(
    entity: Entity,
    servicesDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
    };

    const filePath = path.join(servicesDir, `${entity.name}DataService.cs`);
    await this.writeRenderedTemplate('data-service.generated.cs.hbs', context, filePath, true);
  }
}
