/**
 * C# client generator — generates typed HTTP client libraries (HttpClient or Refit)
 * and C# DTO records that mirror the backend API surface emitted by ControllerGenerator.
 *
 * Activated by blueprint `clients[]` entries with `language: "csharp"`.
 * Writes to `{clientsDir}/{contextName}/{language}-{httpClient}/`.
 */

import { BaseGenerator } from '../BaseGenerator';
import { BoundedContextGenerationContext, Entity, Property } from '../../models/DataModel';
import { ClientConfig } from '../../models/Blueprint';
import { TypeMapper } from '../../utils/TypeMapper';
import { NamingConventions } from '../../utils/NamingConventions';
import * as path from 'path';

interface CsPropContext {
  name: string;
  csType: string;
  nullable: boolean;
  optional: boolean;
}

interface CsEntityContext {
  entityName: string;
  entityNamePlural: string;
  routePrefix: string;
  pkCsType: string;
  pkName: string;
  pkNameLower: string;
  pkRouteParam: string;    // e.g. "{id}" — ready for Refit/route templates
  routeWithPkVar: string;   // C# interpolated segment e.g. "/api/products/{id}"
  routeWithDtoPk: string;   // C# interpolated segment e.g. "/api/products/{dto.Id}"
  createDtoName: string;
  updateDtoName: string;
  deleteDtoName: string;
  detailsDtoName: string;
  searchDtoName: string;
  projectionName: string;
  createProps: CsPropContext[];
  updateProps: CsPropContext[];
  deleteProps: CsPropContext[];
  detailsProps: CsPropContext[];
  searchProps: CsPropContext[];
  projectionProps: CsPropContext[];
}

export class CSharpClientGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const clients = (this.blueprint?.boundedContext.clients ?? []).filter(
      (c): c is ClientConfig & { language: 'csharp' } => c.language === 'csharp'
    );

    if (clients.length === 0) return;

    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const regularEntities = model.entities.filter((e) => !e.isJunction);

    for (const clientCfg of clients) {
      const httpClient = clientCfg.httpClient as 'httpclient' | 'refit';
      const clientNamespace = `${namespace}.Client`;
      const outDir = this.resolveClientOutputDir(contextName, clientCfg);

      const entityContexts = regularEntities.map((e) => this.buildEntityContext(e));

      // Generate per-entity client file
      for (const entityCtx of entityContexts) {
        const fileName =
          httpClient === 'refit'
            ? `I${entityCtx.entityName}Client.cs`
            : `${entityCtx.entityName}HttpClient.cs`;

        const filePath = path.join(outDir, fileName);
        await this.writeRenderedTemplate(
          `clients/csharp/${httpClient}/client.cs.hbs`,
          { ...entityCtx, contextName, namespace: clientNamespace, httpClient },
          filePath,
          true
        );
      }

      // Generate shared DTOs file
      const dtosFilePath = path.join(outDir, `${contextName}ClientDtos.cs`);
      await this.writeRenderedTemplate(
        'clients/csharp/shared/dtos.cs.hbs',
        { contextName, namespace: clientNamespace, entities: entityContexts, httpClient },
        dtosFilePath,
        true
      );

      // Generate DI extension
      const diFilePath = path.join(outDir, `${contextName}ClientServiceCollectionExtensions.cs`);
      await this.writeRenderedTemplate(
        'clients/csharp/shared/di-extensions.cs.hbs',
        {
          contextName,
          namespace: clientNamespace,
          entities: entityContexts,
          httpClient,
          isRefit: httpClient === 'refit',
          isHttpClient: httpClient === 'httpclient',
        },
        diFilePath,
        true
      );
    }
  }

  // ---------------------------------------------------------------------------

  private resolveClientOutputDir(contextName: string, cfg: ClientConfig): string {
    if (cfg.outputSubDir) {
      return path.join(this.metadata?.clientsDir ?? 'generated-clients', cfg.outputSubDir);
    }
    return path.join(
      this.metadata?.clientsDir ?? 'generated-clients',
      contextName,
      `${cfg.language}-${cfg.httpClient}`
    );
  }

  private buildEntityContext(entity: Entity): CsEntityContext {
    const entityName = entity.name;
    const entityNamePlural = this.pluralize(entityName);
    const routePrefix = `api/${entityNamePlural.toLowerCase()}`;

    const pkName = entity.pk?.name ?? 'Id';
    const pkCsType = TypeMapper.toCSharpType(entity.pk?.type ?? 'Guid');
    const pkNameLower = NamingConventions.toCamelCase(pkName);

    const createDtoName = NamingConventions.toCreateDtoName(entityName);
    const updateDtoName = NamingConventions.toUpdateDtoName(entityName);
    const deleteDtoName = NamingConventions.toDeleteDtoName(entityName);
    const detailsDtoName = NamingConventions.toDetailsDtoName(entityName);
    const searchDtoName = NamingConventions.toSearchDtoName(entityName);
    const projectionName = `${entityName}Projection`;

    const scalarProps = entity.properties.filter(
      (p) => !p.isCollection && !(p.isForeignKey && p.navigationProperty)
    );

    const createProps = this.buildProps(
      scalarProps.filter((p) => this.includeInCreate(p))
    );
    const updateProps = this.buildProps(
      scalarProps.filter((p) => this.includeInUpdate(p))
    );
    const deleteProps: CsPropContext[] = [
      { name: pkName, csType: pkCsType, nullable: false, optional: false },
    ];
    const detailsProps = this.buildProps(scalarProps);
    const searchProps = this.buildProps(
      scalarProps.filter((p) => this.isSearchable(p))
    );
    const projectionProps = this.buildProps(
      scalarProps.map((p) => ({ ...p, required: false }))
    );

    return {
      entityName,
      entityNamePlural,
      routePrefix,
      pkCsType,
      pkName,
      pkNameLower,
      pkRouteParam: `{${pkNameLower}}`,
      routeWithPkVar: `/${routePrefix}/{${pkNameLower}}`,
      routeWithDtoPk: `/${routePrefix}/{dto.${pkName}}`,
      createDtoName,
      updateDtoName,
      deleteDtoName,
      detailsDtoName,
      searchDtoName,
      projectionName,
      createProps,
      updateProps,
      deleteProps,
      detailsProps,
      searchProps,
      projectionProps,
    };
  }

  private buildProps(props: Property[]): CsPropContext[] {
    return props.map((p) => {
      const nullable = !p.required;
      return {
        name: p.name,
        csType: TypeMapper.toCSharpType(p.enumType ?? p.type, nullable),
        nullable,
        optional: !p.required,
      };
    });
  }

  private includeInCreate(p: Property): boolean {
    if (p.includeInDto?.create === false) return false;
    return !p.isForeignKey || p.includeInDto?.create === true;
  }

  private includeInUpdate(p: Property): boolean {
    if (p.includeInDto?.update === false) return false;
    return !p.isForeignKey || p.includeInDto?.update === true;
  }

  private isSearchable(p: Property): boolean {
    const t = (p.type ?? '').toLowerCase();
    return (
      t === 'string' ||
      t === 'guid' ||
      t === 'uuid' ||
      t === 'int' ||
      t === 'long' ||
      t === 'bool' ||
      t === 'boolean' ||
      p.isForeignKey === true
    );
  }

  private pluralize(word: string): string {
    if (word.endsWith('y')) return word.slice(0, -1) + 'ies';
    if (word.endsWith('s') || word.endsWith('x') || word.endsWith('ch')) return word + 'es';
    return word + 's';
  }
}
