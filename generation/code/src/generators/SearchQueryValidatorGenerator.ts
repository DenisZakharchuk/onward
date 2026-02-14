/**
 * SearchQueryValidatorGenerator - generates search query validators
 * Now generates simple derived classes from BaseSearchQueryValidator
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity } from '../models/DataModel';
import * as path from 'path';

export class SearchQueryValidatorGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const validatorsDir = `${baseNamespace}.${contextName}.Domain/Validators`;

    for (const entity of model.entities) {
      // Skip junction entities
      if (entity.isJunction) {
        continue;
      }

      await this.generateValidator(entity, validatorsDir, namespace, baseNamespace);
    }
  }

  private async generateValidator(
    entity: Entity,
    validatorsDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const context = {
      namespace,
      baseNamespace,
      entityName: entity.name,
      description: entity.description || entity.name,
    };

    const filePath = path.join(validatorsDir, `${entity.name}SearchQueryValidator.cs`);
    await this.writeRenderedTemplate(
      'search-query-validator.generated.cs.hbs',
      context,
      filePath,
      true // Overwrite allowed
    );
  }
}
