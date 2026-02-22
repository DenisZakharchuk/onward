/**
 * File-based model provider implementation
 */

import { IModelProvider } from '../abstractions/IModelProvider';
import { DomainModel } from '../models/DataModel';
import { DataModelParser } from '../parser/DataModelParser';

export class FileModelProvider implements IModelProvider {
  private parser: DataModelParser;

  constructor() {
    this.parser = new DataModelParser();
  }

  async load(source: string): Promise<DomainModel> {
    await this.parser.loadSchema();
    return await this.parser.parseFromFile(source);
  }

  async validate(source: string): Promise<{
    valid: boolean;
    errors: string[];
    model?: DomainModel;
  }> {
    try {
      await this.parser.loadSchema();
      const model = await this.parser.parseFromFile(source);
      return {
        valid: true,
        errors: [],
        model,
      };
    } catch (error) {
      return {
        valid: false,
        errors: [error instanceof Error ? error.message : String(error)],
      };
    }
  }
}
