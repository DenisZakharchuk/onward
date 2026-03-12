/**
 * DI generator - creates service registration extensions in <Context>.DI
 */

import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext } from '../models/DataModel';
import { NamingConventions } from '../utils/NamingConventions';
import * as path from 'path';

export class DiGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
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
        isOwned: e.owned === true && model.boundedContext.ownership?.enabled === true,
      }));

    const ownershipCfg = model.boundedContext.ownership;
    const hasOwnership = ownershipCfg?.enabled === true;
    const ownershipValueObject = ownershipCfg?.valueObject ?? 'UserTenantOwnership';
    const ownershipFactory = ownershipCfg?.factory ?? `${ownershipValueObject}Factory`;

    // Derive idempotency template flags ----------------------------------------
    // Any entity with versioned='rowversion' implies data-level idempotency is active,
    // which requires the HTTP token accessor even when the context-level mode is unset.
    const anyEntityVersioned = model.entities.some(e => e.versioned === 'rowversion');
    const idempotencyMode = model.boundedContext.idempotency?.mode
      ?? (anyEntityVersioned ? 'data' : 'none');
    const cacheMode = model.boundedContext.idempotency?.cache?.mode;

    const idempotency = {
      mode: idempotencyMode,
      // Register HttpContextIdempotencyTokenAccessor when any real idempotency is active
      hasHttpTokenAccessor: idempotencyMode !== 'none',
      isInMemoryCache: idempotencyMode === 'cache' && (cacheMode === 'inmemory' || !cacheMode),
      isDistributedCache: idempotencyMode === 'cache' && cacheMode === 'distributed',
    };
    // ---------------------------------------------------------------------------

    const context = {
      baseNamespace,
      namespace,
      contextName,
      entities,
      hasOwnership,
      ownershipValueObject,
      ownershipFactory,
      idempotency,
    };

    const filePath = path.join(extensionsDir, `${contextName}ServiceCollectionExtensions.cs`);
    await this.writeRenderedTemplate(
      ['di/service-collection/ef-core.generated.cs.hbs', 'di-service-collection.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }
}
