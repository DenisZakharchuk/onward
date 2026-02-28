import * as path from 'path';
import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext } from '../models/DataModel';
import { AuthModeResolver } from '../utils/AuthModeResolver';

export class AdoNetMinimalApiProgramGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const apiProjectPath = `${baseNamespace}.${contextName}.API`;

    const context = {
      namespace,
      contextName,
      contextDescription: model.boundedContext.description || `${contextName} API`,
      authorizationEnabled: AuthModeResolver.isAuthorizationEnabled(this.blueprint),
    };

    const filePath = path.join(apiProjectPath, 'Program.cs');
    await this.writeRenderedTemplate(
      ['api/program/minimal.ado-net.generated.cs.hbs', 'minimal-api-program.ado-net.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }
}
