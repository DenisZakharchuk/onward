/**
 * Data model parser and validator
 */

import Ajv, { ValidateFunction, Schema } from 'ajv';
import addFormats from 'ajv-formats';
import { DataModel } from '../models/DataModel';
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
   * Parse and validate data model from JSON file
   */
  async parseFromFile(filePath: string): Promise<DataModel> {
    const data = await FileManager.readJson<DataModel>(filePath);
    return this.parse(data);
  }

  /**
   * Parse and validate data model from object
   */
  parse(data: unknown): DataModel {
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
    this.validateBusinessRules(data as DataModel);

    return data as DataModel;
  }

  /**
   * Validate business rules not covered by JSON Schema
   */
  private validateBusinessRules(model: DataModel): void {
    const entityNames = new Set<string>();

    // Check for duplicate entity names
    for (const entity of model.entities) {
      if (entityNames.has(entity.name)) {
        throw new Error(`Duplicate entity name: ${entity.name}`);
      }
      entityNames.add(entity.name);
    }

    // Validate relationships
    if (model.relationships) {
      for (const rel of model.relationships) {
        // Check that referenced entities exist
        if (!entityNames.has(rel.leftEntity)) {
          throw new Error(
            `Relationship references unknown entity: ${rel.leftEntity}`
          );
        }
        if (!entityNames.has(rel.rightEntity)) {
          throw new Error(
            `Relationship references unknown entity: ${rel.rightEntity}`
          );
        }

        // Check ManyToMany has junction entity
        if (rel.type === 'ManyToMany') {
          if (!rel.junctionEntity) {
            throw new Error(
              `ManyToMany relationship between ${rel.leftEntity} and ${rel.rightEntity} must specify junctionEntity`
            );
          }
          if (!entityNames.has(rel.junctionEntity)) {
            throw new Error(
              `ManyToMany relationship references unknown junction entity: ${rel.junctionEntity}`
            );
          }
        }
      }
    }

    // Validate junction entities have proper metadata
    for (const entity of model.entities) {
      if (entity.isJunction && !entity.junctionMetadata) {
        throw new Error(
          `Junction entity ${entity.name} must have junctionMetadata`
        );
      }

      if (entity.junctionMetadata) {
        if (!entityNames.has(entity.junctionMetadata.leftEntity)) {
          throw new Error(
            `Junction entity ${entity.name} references unknown leftEntity: ${entity.junctionMetadata.leftEntity}`
          );
        }
        if (!entityNames.has(entity.junctionMetadata.rightEntity)) {
          throw new Error(
            `Junction entity ${entity.name} references unknown rightEntity: ${entity.junctionMetadata.rightEntity}`
          );
        }
      }
    }

    // Validate foreign keys reference existing entities
    for (const entity of model.entities) {
      for (const prop of entity.properties) {
        if (prop.isForeignKey && prop.referencedEntity) {
          if (!entityNames.has(prop.referencedEntity)) {
            throw new Error(
              `Property ${entity.name}.${prop.name} references unknown entity: ${prop.referencedEntity}`
            );
          }
        }

        // Validate collection types
        if (prop.isCollection && prop.collectionType) {
          if (!entityNames.has(prop.collectionType)) {
            throw new Error(
              `Property ${entity.name}.${prop.name} has unknown collectionType: ${prop.collectionType}`
            );
          }
        }

        // Validate enum types
        if (prop.enumType) {
          const enumExists = model.enums?.some((e) => e.name === prop.enumType);
          if (!enumExists) {
            throw new Error(
              `Property ${entity.name}.${prop.name} references unknown enum: ${prop.enumType}`
            );
          }
        }
      }
    }

    // Validate enum value uniqueness
    if (model.enums) {
      for (const enumDef of model.enums) {
        const valueSet = new Set<number>();
        const nameSet = new Set<string>();

        for (const enumValue of enumDef.values) {
          if (nameSet.has(enumValue.name)) {
            throw new Error(
              `Duplicate enum value name in ${enumDef.name}: ${enumValue.name}`
            );
          }
          nameSet.add(enumValue.name);

          if (enumValue.value !== undefined) {
            if (valueSet.has(enumValue.value)) {
              throw new Error(
                `Duplicate enum value in ${enumDef.name}: ${enumValue.value}`
              );
            }
            valueSet.add(enumValue.value);
          }
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
      this.validateBusinessRules(data as DataModel);
      return [];
    } catch (error) {
      return [error instanceof Error ? error.message : String(error)];
    }
  }
}
