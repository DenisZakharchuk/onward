import Ajv, { Schema, ValidateFunction } from 'ajv';
import addFormats from 'ajv-formats';
import * as path from 'path';
import { Blueprint } from '../models/Blueprint';
import { FileManager } from '../utils/FileManager';

export class BlueprintParser {
  private ajv: Ajv;
  private validator: ValidateFunction | null = null;

  constructor() {
    this.ajv = new Ajv({ allErrors: true, strict: false });
    addFormats(this.ajv);
  }

  async loadSchema(schemaPath?: string): Promise<void> {
    const defaultSchemaPath = path.join(__dirname, '../../schemas/blueprint.schema.json');
    const resolvedPath = schemaPath || defaultSchemaPath;

    const schema = await FileManager.readJson<Schema>(resolvedPath);
    this.validator = this.ajv.compile(schema);
  }

  async parseFromFile(filePath: string): Promise<Blueprint> {
    const data = await FileManager.readJson<Blueprint>(filePath);
    return this.parse(data);
  }

  parse(data: unknown): Blueprint {
    if (!this.validator) {
      throw new Error('Blueprint schema not loaded. Call loadSchema() first.');
    }

    const valid = this.validator(data);

    if (!valid) {
      const errors = this.validator.errors || [];
      const errorMessages = errors.map((err) => `${err.instancePath} ${err.message}`);
      throw new Error(`Blueprint validation failed:\n${errorMessages.join('\n')}`);
    }

    return data as Blueprint;
  }
}