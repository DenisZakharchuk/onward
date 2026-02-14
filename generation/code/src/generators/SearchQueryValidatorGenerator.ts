/**
 * SearchQueryValidatorGenerator - generates search query validators
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property } from '../models/DataModel';
import { TypeMapper } from '../utils/TypeMapper';
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

      await this.generateValidator(entity, validatorsDir, namespace);
    }
  }

  private async generateValidator(
    entity: Entity,
    validatorsDir: string,
    namespace: string
  ): Promise<void> {
    const properties = entity.properties.filter((p) => !p.isCollection);

    const context = {
      namespace,
      entityName: entity.name,
      description: entity.description || entity.name,
      properties: this.buildPropertyValidationContext(properties),
    };

    const filePath = path.join(validatorsDir, `${entity.name}SearchQueryValidator.cs`);
    await this.writeRenderedTemplate(
      'search-query-validator.generated.cs.hbs',
      context,
      filePath,
      true // Overwrite allowed
    );
  }

  private buildPropertyValidationContext(properties: Property[]): Array<{
    name: string;
    csharpType: string;
    isString: boolean;
    isNumeric: boolean;
    isDateTime: boolean;
    isBoolean: boolean;
    isGuid: boolean;
    isEnum: boolean;
    enumType: string | null;
    nullable: boolean;
  }> {
    return properties.map((p) => {
      const csharpType = TypeMapper.toCSharpType(p.type, false);
      
      return {
        name: p.name,
        csharpType,
        isString: csharpType === 'string',
        isNumeric: ['int', 'long', 'decimal', 'double', 'float'].includes(csharpType),
        isDateTime: csharpType === 'DateTime',
        isBoolean: csharpType === 'bool',
        isGuid: csharpType === 'Guid',
        isEnum: !!p.enumType,
        enumType: p.enumType || null,
        nullable: !p.required,
      };
    });
  }
}
