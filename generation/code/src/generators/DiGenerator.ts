/**
 * DI generator - creates service registration extensions in <Context>.DI
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel } from '../models/DataModel';
import { NamingConventions } from '../utils/NamingConventions';
import * as path from 'path';

export class DiGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const diProjectPath = `${baseNamespace}.${contextName}.DI`;
    const extensionsDir = path.join(diProjectPath, 'Extensions');

    const entities = model.entities
      .filter((e) => !e.isJunction)
      .map((e) => ({
        entityName: e.name,
        createDtoName: NamingConventions.toCreateDtoName(e.name),
        updateDtoName: NamingConventions.toUpdateDtoName(e.name),
        detailsDtoName: NamingConventions.toDetailsDtoName(e.name),
        searchDtoName: NamingConventions.toSearchDtoName(e.name),
        projectionName: `${e.name}Projection`,
        mapperName: NamingConventions.toMapperName(e.name),
        creatorName: NamingConventions.toCreatorName(e.name),
        modifierName: NamingConventions.toModifierName(e.name),
        searchProviderName: NamingConventions.toSearchProviderName(e.name),
        createValidatorName: NamingConventions.toValidatorName(e.name, 'Create'),
        updateValidatorName: NamingConventions.toValidatorName(e.name, 'Update'),
        queryBuilderName: `${e.name}QueryBuilder`,
        projectionMapperName: `${e.name}ProjectionMapper`,
        searchServiceName: `${e.name}SearchService`,
        searchQueryValidatorName: `${e.name}SearchQueryValidator`,
        dataServiceInterfaceName: NamingConventions.toDataServiceInterfaceName(e.name),
        dataServiceName: NamingConventions.toDataServiceClassName(e.name),
      }));

    const context = {
      baseNamespace,
      namespace,
      contextName,
      entities,
    };

    const filePath = path.join(extensionsDir, `${contextName}ServiceCollectionExtensions.cs`);
    await this.writeRenderedTemplate('di-service-collection.generated.cs.hbs', context, filePath, true);
  }
}
