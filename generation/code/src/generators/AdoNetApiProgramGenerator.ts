import * as path from 'path';
import { BaseGenerator } from './BaseGenerator';
import { DataModel } from '../models/DataModel';

export class AdoNetApiProgramGenerator extends BaseGenerator {
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
      ['api/program/controllers.ado-net.generated.cs.hbs', 'api-program.ado-net.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }
}
