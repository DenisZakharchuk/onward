import { Blueprint } from '../models/Blueprint';
import { BlueprintParser } from '../parser/BlueprintParser';

export class FileBlueprintProvider {
  private parser: BlueprintParser;

  constructor() {
    this.parser = new BlueprintParser();
  }

  async load(source: string): Promise<Blueprint> {
    await this.parser.loadSchema();
    return this.parser.parseFromFile(source);
  }

  async validate(source: string): Promise<{
    valid: boolean;
    errors: string[];
    blueprint?: Blueprint;
  }> {
    try {
      await this.parser.loadSchema();
      const blueprint = await this.parser.parseFromFile(source);

      return {
        valid: true,
        errors: [],
        blueprint,
      };
    } catch (error) {
      return {
        valid: false,
        errors: [error instanceof Error ? error.message : String(error)],
      };
    }
  }
}