/**
 * Generator for perContext auth endpoints.
 * Emits an AuthController, auth DTOs, and a service interface stub
 * when the blueprint uses authorization.mode = 'perContext'.
 */

import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext } from '../models/DataModel';
import { AuthModeResolver } from '../utils/AuthModeResolver';
import * as path from 'path';

export class PerContextAuthEndpointsGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    if (!AuthModeResolver.isPerContextAuth(this.blueprint)) {
      // Only run for perContext mode — skip silently for all other modes
      return;
    }

    const contextName = model.boundedContext.name;
    const namespace =  model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const context = {
      namespace,
      contextName,
      baseNamespace,
    };

    // 1. AuthController  (API project / Controllers folder)
    const controllerPath = path.join(
      `${baseNamespace}.${contextName}.API`,
      'Controllers',
      'AuthController.cs'
    );
    await this.writeRenderedTemplate(
      'api/auth/per-context-auth-controller.generated.cs.hbs',
      context,
      controllerPath,
      true
    );

    // 2. Auth DTOs  (DTO project)
    const dtosPath = path.join(
      `${baseNamespace}.${contextName}.DTO`,
      'DTO',
      'Auth',
      `${contextName}AuthDTOs.cs`
    );
    await this.writeRenderedTemplate(
      'api/auth/per-context-auth-dtos.generated.cs.hbs',
      context,
      dtosPath,
      true
    );

    // 3. Service interface  (BL project / Services / Abstractions folder)
    const interfacePath = path.join(
      `${baseNamespace}.${contextName}.BL`,
      'Services',
      'Abstractions',
      `I${contextName}AuthenticationService.cs`
    );
    await this.writeRenderedTemplate(
      'api/auth/per-context-auth-service-interface.generated.cs.hbs',
      context,
      interfacePath,
      true
    );
  }
}
