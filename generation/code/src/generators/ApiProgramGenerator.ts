/**
 * API Program generator - creates Program.cs in API project
 */

import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext } from '../models/DataModel';
import { AuthModeResolver } from '../utils/AuthModeResolver';
import * as path from 'path';

export class ApiProgramGenerator extends BaseGenerator {
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
      onlineAuthEnabled: AuthModeResolver.isOnlineAuth(this.blueprint),
      onlineAuth: AuthModeResolver.resolveOnlineAuthConfig(this.blueprint, model.boundedContext),
    };

    const filePath = path.join(apiProjectPath, 'Program.cs');
    await this.writeRenderedTemplate(
      ['api/program/controllers.generated.cs.hbs', 'api-program.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }
}
