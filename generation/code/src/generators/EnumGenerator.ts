import * as path from 'path';
import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext, EnumDefinition, EnumValue } from '../models/DataModel';

/**
 * Generator for creating C# enum files in the Common project
 */
export class EnumGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    if (!model.enums || model.enums.length === 0) {
      console.log('  No enums to generate');
      return;
    }

    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';
    const contextName = model.boundedContext.name;
    const commonProjectPath = `${baseNamespace}.${contextName}.Common`;

    // Ensure Enums directory exists
    const enumsDir = path.join(commonProjectPath, 'Enums');

    // Generate each enum
    for (const enumDef of model.enums) {
      await this.generateEnum(enumDef, enumsDir, namespace);
    }
  }

  private async generateEnum(
    enumDef: EnumDefinition,
    enumsDir: string,
    namespace: string
  ): Promise<void> {
    const context = {
      namespace,
      name: enumDef.name,
      description: enumDef.description,
      values: enumDef.values.map((v: EnumValue) => ({
        name: v.name,
        value: v.value,
        description: v.description || '',
      })),
    };

    const filePath = path.join(enumsDir, `${enumDef.name}.cs`);
    await this.writeRenderedTemplate(
      ['common/enum.hbs', 'enum.cs.hbs'],
      context,
      filePath,
      true
    );
  }
}
