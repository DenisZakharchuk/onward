/**
 * TypeScript client generator — generates typed HTTP client libraries and TypeScript
 * DTO interfaces that mirror the backend API surface emitted by ControllerGenerator.
 *
 * Activated by blueprint `clients[]` entries with `language: "typescript"`.
 * Writes to `{clientsDir}/{contextName}/{language}-{httpClient}/`.
 */

import { BaseGenerator } from '../BaseGenerator';
import { BoundedContextGenerationContext, Entity, Property } from '../../models/DataModel';
import { ClientConfig } from '../../models/Blueprint';
import { TypeScriptTypeMapper } from '../../utils/TypeScriptTypeMapper';
import { NamingConventions } from '../../utils/NamingConventions';
import * as path from 'path';

interface TsPropContext {
  name: string;
  tsType: string;
  nullable: boolean;
  optional: boolean;   // true → field is `?: T` in the interface
}

interface TsEntityContext {
  entityName: string;
  entityNameLower: string;
  entityNamePlural: string;
  routePrefix: string;
  pkTsType: string;
  pkName: string;
  pkNameLower: string;
  createDtoName: string;
  updateDtoName: string;
  deleteDtoName: string;
  detailsDtoName: string;
  searchDtoName: string;
  projectionName: string;
  createProps: TsPropContext[];
  updateProps: TsPropContext[];
  deleteProps: TsPropContext[];
  detailsProps: TsPropContext[];
  searchProps: TsPropContext[];
  projectionProps: TsPropContext[];
}

export class TypeScriptClientGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const clients = (this.blueprint?.boundedContext.clients ?? []).filter(
      (c): c is ClientConfig & { language: 'typescript' } => c.language === 'typescript'
    );

    if (clients.length === 0) return;

    const contextName = model.boundedContext.name;
    const regularEntities = model.entities.filter((e) => !e.isJunction);

    for (const clientCfg of clients) {
      const httpClient = clientCfg.httpClient as string;
      const outDir = this.resolveClientOutputDir(contextName, clientCfg);

      const entityContexts = regularEntities.map((e) => this.buildEntityContext(e));

      // Generate per-entity client file
      for (const entityCtx of entityContexts) {
        const filePath = path.join(outDir, `${entityCtx.entityName}Client.ts`);
        await this.writeRenderedTemplate(
          `clients/typescript/${httpClient}/client.ts.hbs`,
          { ...entityCtx, contextName, httpClient },
          filePath,
          true
        );
      }

      // Generate shared types file
      const typesFilePath = path.join(outDir, 'types.ts');
      await this.writeRenderedTemplate(
        'clients/typescript/types.ts.hbs',
        { contextName, entities: entityContexts, httpClient },
        typesFilePath,
        true
      );

      // Generate barrel index
      const indexFilePath = path.join(outDir, 'index.ts');
      await this.writeRenderedTemplate(
        'clients/typescript/index.ts.hbs',
        {
          contextName,
          entityNames: entityContexts.map((e) => e.entityName),
          httpClient,
        },
        indexFilePath,
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

  private buildEntityContext(entity: Entity): TsEntityContext {
    const entityName = entity.name;
    const entityNameLower = NamingConventions.toCamelCase(entityName);
    const entityNamePlural = this.pluralize(entityNameLower);
    const routePrefix = `api/${entityNamePlural}`;

    const pkName = entity.pk?.name ?? 'Id';
    const pkTsType = TypeScriptTypeMapper.toTsType(entity.pk?.type ?? 'Guid');
    const pkNameLower = NamingConventions.toCamelCase(pkName);

    const createDtoName = NamingConventions.toCreateDtoName(entityName);
    const updateDtoName = NamingConventions.toUpdateDtoName(entityName);
    const deleteDtoName = NamingConventions.toDeleteDtoName(entityName);
    const detailsDtoName = NamingConventions.toDetailsDtoName(entityName);
    const searchDtoName = NamingConventions.toSearchDtoName(entityName);
    const projectionName = `${entityName}Projection`;

    // Scalar properties only (no navigation / collection)
    const scalarProps = entity.properties.filter(
      (p) => !p.isCollection && !(p.isForeignKey && p.navigationProperty)
    );

    const createProps = this.buildProps(
      scalarProps.filter((p) => this.includeInCreate(p))
    );
    const updateProps = this.buildProps(
      scalarProps.filter((p) => this.includeInUpdate(p))
    );
    const deleteProps: TsPropContext[] = [
      { name: 'id', tsType: pkTsType, nullable: false, optional: false },
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
      entityNameLower,
      entityNamePlural,
      routePrefix,
      pkTsType,
      pkName,
      pkNameLower,
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

  private buildProps(props: Property[]): TsPropContext[] {
    return props.map((p) => {
      const nullable = !p.required;
      return {
        name: NamingConventions.toCamelCase(p.name),
        tsType: TypeScriptTypeMapper.toTsType(p.enumType ?? p.type, nullable),
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
