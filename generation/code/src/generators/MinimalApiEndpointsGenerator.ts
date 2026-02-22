import * as path from 'path';
import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext, Entity } from '../models/DataModel';

export class MinimalApiEndpointsGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const endpointsDir = `${baseNamespace}.${contextName}.API/Endpoints`;

    for (const entity of model.entities) {
      if (entity.isJunction) {
        continue;
      }

      await this.generateEndpointStub(entity, endpointsDir, namespace, baseNamespace);
    }
  }

  private async generateEndpointStub(
    entity: Entity,
    endpointsDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const pluralEntityName = this.pluralize(entity.name);

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      pluralEntityName,
      routePrefix: `api/${this.toKebabCase(pluralEntityName)}`,
    };

    const filePath = path.join(endpointsDir, `${pluralEntityName}MinimalApiEndpoints.generated.cs`);
    await this.writeRenderedTemplate(
      ['api/endpoints/minimal.generated.cs.hbs', 'minimal-api-endpoints.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }

  private pluralize(word: string): string {
    if (word.endsWith('y')) {
      return word.slice(0, -1) + 'ies';
    }

    if (word.endsWith('s') || word.endsWith('x') || word.endsWith('ch') || word.endsWith('sh')) {
      return word + 'es';
    }

    return word + 's';
  }

  private toKebabCase(value: string): string {
    return value
      .replace(/([a-z0-9])([A-Z])/g, '$1-$2')
      .replace(/\s+/g, '-')
      .toLowerCase();
  }
}