import * as path from 'path';
import { BaseGenerator } from './BaseGenerator';
import { DataModel } from '../models/DataModel';

export class MinimalApiProgramGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const apiProjectPath = `${baseNamespace}.${contextName}.API`;

    const context = {
      namespace,
      contextName,
      contextDescription: model.boundedContext.description || `${contextName} API`,
    };

    const filePath = path.join(apiProjectPath, 'Program.cs');
    await this.writeRenderedTemplate(
      ['api/program/minimal.generated.cs.hbs', 'minimal-api-program.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }
}