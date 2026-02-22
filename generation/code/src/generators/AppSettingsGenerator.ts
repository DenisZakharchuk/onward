/**
 * AppSettings generator - creates appsettings.json and appsettings.Development.json in API project
 */

import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext } from '../models/DataModel';
import * as path from 'path';

export class AppSettingsGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const contextName = model.boundedContext.name;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const apiProjectPath = `${baseNamespace}.${contextName}.API`;

    const databaseName =
      model.boundedContext.databaseName ?? contextName.toLowerCase() + '_db';
    const dbPort = model.boundedContext.dbPort ?? 5432;
    const apiPort = model.boundedContext.apiPort ?? 5000;
    const jwt = model.boundedContext.jwt;

    const context = {
      contextName,
      databaseName,
      dbPort,
      apiPort,
      jwt,
    };

    // Write appsettings.json — skip if already exists; regenerate with --force
    await this.writeRenderedTemplate(
      'api/appsettings/appsettings.json.hbs',
      context,
      path.join(apiProjectPath, 'appsettings.json'),
      false
    );

    // Write appsettings.Development.json — skip if already exists
    await this.writeRenderedTemplate(
      'api/appsettings/appsettings.Development.json.hbs',
      context,
      path.join(apiProjectPath, 'appsettings.Development.json'),
      false
    );

    // Write Properties/launchSettings.json — sets ASPNETCORE_ENVIRONMENT=Development
    // so `dotnet run` enables Swagger by default
    await this.writeRenderedTemplate(
      'api/appsettings/launchSettings.json.hbs',
      context,
      path.join(apiProjectPath, 'Properties', 'launchSettings.json'),
      false
    );
  }
}
