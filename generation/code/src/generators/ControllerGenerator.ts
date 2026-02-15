/**
 * Controller generator - creates API controllers
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity } from '../models/DataModel';
import * as path from 'path';

export class ControllerGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const controllersDir = `${baseNamespace}.${contextName}.API/Controllers`;

    for (const entity of model.entities) {
      // Skip junction entities - they use relationship endpoints on parent entities
      if (entity.isJunction) {
        continue;
      }

      await this.generateController(entity, controllersDir, namespace, baseNamespace);
    }
  }

  private async generateController(
    entity: Entity,
    controllersDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const pluralEntityName = this.pluralize(entity.name);

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      pluralEntityName,
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
