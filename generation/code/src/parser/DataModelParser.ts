/**
 * Data model parser and validator
 */

import Ajv, { ValidateFunction, Schema } from 'ajv';
import addFormats from 'ajv-formats';
import {
  DomainModel,
  BoundedContext,
  BoundedContextGenerationContext,
  EnumDefinition,
} from '../models/DataModel';
import { FileManager } from '../utils/FileManager';
import * as path from 'path';

export class DataModelParser {
  private ajv: Ajv;
  private validator: ValidateFunction | null = null;

  constructor() {
    this.ajv = new Ajv({ allErrors: true, strict: false });
    addFormats(this.ajv);
  }

  /**
   * Load and compile the JSON schema
   */
  async loadSchema(schemaPath?: string): Promise<void> {
    const defaultSchemaPath = path.join(
      __dirname,
      '../../schemas/data-model.schema.json'
    );
    const resolvedPath = schemaPath || defaultSchemaPath;

    const schema = await FileManager.readJson<Schema>(resolvedPath);
    this.validator = this.ajv.compile(schema);
  }

  /**
   * Parse and validate domain model from JSON file
   */
  async parseFromFile(filePath: string): Promise<DomainModel> {
    const data = await FileManager.readJson<DomainModel>(filePath);
    return this.parse(data);
  }

  /**
   * Parse and validate domain model from object
   */
  parse(data: unknown): DomainModel {
    if (!this.validator) {
      throw new Error('Schema not loaded. Call loadSchema() first.');
    }

    const valid = this.validator(data);

    if (!valid) {
      const errors = this.validator.errors || [];
      const errorMessages = errors.map(
        (err) => `${err.instancePath} ${err.message}`
      );
      throw new Error(
        `Data model validation failed:\n${errorMessages.join('\n')}`
      );
    }

    // Perform business logic validation
    this.validateBusinessRules(data as DomainModel);

    return data as DomainModel;
  }

  /**
   * Build per-context generation contexts by merging domain-level and context-level enums.
   * This is the single place that flattens DomainModel → BoundedContextGenerationContext[].
   */
  buildGenerationContexts(domain: DomainModel): BoundedContextGenerationContext[] {
    return domain.boundedContexts.map((ctx) => {
      const mergedEnums = mergeEnums(domain.enums ?? [], ctx.enums ?? []);
      return {
        boundedContext: ctx,
        enums: mergedEnums,
        entities: ctx.dataModel.entities,
        relationships: ctx.dataModel.relationships ?? [],
      };
    });
  }

  /**
   * Validate business rules not covered by JSON Schema
   */
  private validateBusinessRules(domain: DomainModel): void {
    // Cross-context: no duplicate bounded context names
    const contextNames = new Set<string>();
    for (const ctx of domain.boundedContexts) {
      if (contextNames.has(ctx.name)) {
        throw new Error(`Duplicate boundedContext name: ${ctx.name}`);
      }
      contextNames.add(ctx.name);
    }

    // Validate domain-level enums
    if (domain.enums) {
      this.validateEnums(domain.enums, 'domain');
    }

    // Per-context validation
    for (const ctx of domain.boundedContexts) {
      const mergedEnums = mergeEnums(domain.enums ?? [], ctx.enums ?? []);
      this.validateBoundedContext(ctx, mergedEnums);
    }
  }

  /**
   * Validate a single bounded context
   */
  private validateBoundedContext(
    ctx: BoundedContext,
    mergedEnums: EnumDefinition[]
  ): void {
    const { dataModel, name: ctxName } = ctx;
    const enumNames = new Set(mergedEnums.map((e) => e.name));
    const entityNames = new Set<string>();

    // Validate context-level enums
    if (ctx.enums) {
      this.validateEnums(ctx.enums, ctxName);
    }

    // Check for duplicate entity names
    for (const entity of dataModel.entities) {
      if (entityNames.has(entity.name)) {
        throw new Error(`[${ctxName}] Duplicate entity name: ${entity.name}`);
      }
      entityNames.add(entity.name);
    }

    // Validate relationships
    if (dataModel.relationships) {
      for (const rel of dataModel.relationships) {
        if (!entityNames.has(rel.leftEntity)) {
          throw new Error(
            `[${ctxName}] Relationship references unknown entity: ${rel.leftEntity}`
          );
        }
        if (!entityNames.has(rel.rightEntity)) {
          throw new Error(
            `[${ctxName}] Relationship references unknown entity: ${rel.rightEntity}`
          );
        }

        if (rel.type === 'ManyToMany') {
          if (!rel.junctionEntity) {
            throw new Error(
              `[${ctxName}] ManyToMany relationship between ${rel.leftEntity} and ${rel.rightEntity} must specify junctionEntity`
            );
          }
          if (!entityNames.has(rel.junctionEntity)) {
            throw new Error(
              `[${ctxName}] ManyToMany relationship references unknown junction entity: ${rel.junctionEntity}`
            );
          }
        }
      }
    }

    // Validate junction entities have proper metadata
    for (const entity of dataModel.entities) {
      if (entity.isJunction && !entity.junctionMetadata) {
        throw new Error(
          `[${ctxName}] Junction entity ${entity.name} must have junctionMetadata`
        );
      }

      if (entity.junctionMetadata) {
        if (!entityNames.has(entity.junctionMetadata.leftEntity)) {
          throw new Error(
            `[${ctxName}] Junction entity ${entity.name} references unknown leftEntity: ${entity.junctionMetadata.leftEntity}`
          );
        }
        if (!entityNames.has(entity.junctionMetadata.rightEntity)) {
          throw new Error(
            `[${ctxName}] Junction entity ${entity.name} references unknown rightEntity: ${entity.junctionMetadata.rightEntity}`
          );
        }
      }
    }

    // Validate foreign keys and enum references per entity
    for (const entity of dataModel.entities) {
      for (const prop of entity.properties) {
        if (prop.isForeignKey && prop.referencedEntity) {
          if (!entityNames.has(prop.referencedEntity)) {
            throw new Error(
              `[${ctxName}] Property ${entity.name}.${prop.name} references unknown entity: ${prop.referencedEntity}`
            );
          }
        }

        if (prop.isCollection && prop.collectionType) {
          if (!entityNames.has(prop.collectionType)) {
            throw new Error(
              `[${ctxName}] Property ${entity.name}.${prop.name} has unknown collectionType: ${prop.collectionType}`
            );
          }
        }

        if (prop.enumType) {
          if (!enumNames.has(prop.enumType)) {
            throw new Error(
              `[${ctxName}] Property ${entity.name}.${prop.name} references unknown enum: ${prop.enumType}`
            );
          }
        }
      }
    }
  }

  /**
   * Validate enum definitions for duplicate names / values
   */
  private validateEnums(enums: EnumDefinition[], scope: string): void {
    for (const enumDef of enums) {
      const valueSet = new Set<number>();
      const nameSet = new Set<string>();

      for (const enumValue of enumDef.values) {
        if (nameSet.has(enumValue.name)) {
          throw new Error(
            `[${scope}] Duplicate enum value name in ${enumDef.name}: ${enumValue.name}`
          );
        }
        nameSet.add(enumValue.name);

        if (enumValue.value !== undefined) {
          if (valueSet.has(enumValue.value)) {
            throw new Error(
              `[${scope}] Duplicate enum value in ${enumDef.name}: ${enumValue.value}`
            );
          }
          valueSet.add(enumValue.value);
        }
      }
    }
  }

  /**
   * Get validation errors without throwing
   */
  getValidationErrors(data: unknown): string[] {
    if (!this.validator) {
      return ['Schema not loaded'];
    }

    const valid = this.validator(data);

    if (!valid) {
      return (this.validator.errors || []).map(
        (err) => `${err.instancePath} ${err.message}`
      );
    }

    try {
      this.validateBusinessRules(data as DomainModel);
      return [];
    } catch (error) {
      return [error instanceof Error ? error.message : String(error)];
    }
  }
}

/**
 * Merge domain-level enums with context-level enums.
 * Context-level enums override domain-level enums with the same name.
 */
function mergeEnums(
  domainEnums: EnumDefinition[],
  contextEnums: EnumDefinition[]
): EnumDefinition[] {
  const map = new Map<string, EnumDefinition>();
  for (const e of domainEnums) {
    map.set(e.name, e);
  }
  for (const e of contextEnums) {
    map.set(e.name, e); // context wins
  }
  return Array.from(map.values());
}
