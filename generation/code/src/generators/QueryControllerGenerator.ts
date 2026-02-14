/**
 * QueryControllerGenerator - generates ADT query controller classes
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel } from '../models/DataModel';
import { NamingConventions } from '../utils/NamingConventions';
import * as path from 'path';

export class QueryControllerGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const controllersDir = `${baseNamespace}.${contextName}.API/Controllers`;

    for (const entity of model.entities) {
      // Skip junction entities
      if (entity.isJunction) {
        continue;
      }

      const entityNamePlural = this.pluralize(entity.name);
      const routePrefix = `api/${NamingConventions.toCamelCase(entityNamePlural)}/query`;

      const context = {
        namespace,
        entityName: entity.name,
        entityNamePlural,
        projectionName: `${entity.name}Projection`,
        routePrefix,
        description: entity.description || entity.name,
      };

      const filePath = path.join(controllersDir, `${entityNamePlural}QueryController.cs`);
      await this.writeRenderedTemplate(
        'query-controller.generated.cs.hbs',
        context,
        filePath,
        true // Overwrite allowed
      );
    }
  }

  /**
   * Simple pluralization (can be enhanced with inflection library if needed)
   */
  private pluralize(name: string): string {
    // Handle common irregular plurals
    const irregulars: Record<string, string> = {
      Category: 'Categories',
      Person: 'People',
      Child: 'Children',
    };

    if (irregulars[name]) {
      return irregulars[name];
    }

    // Simple rules
    if (name.endsWith('y') && !this.isVowel(name[name.length - 2])) {
      return name.slice(0, -1) + 'ies';
    }
    if (name.endsWith('s') || name.endsWith('x') || name.endsWith('ch') || name.endsWith('sh')) {
      return name + 'es';
    }
    return name + 's';
  }

  private isVowel(char: string): boolean {
    return 'aeiouAEIOU'.includes(char);
  }
}
