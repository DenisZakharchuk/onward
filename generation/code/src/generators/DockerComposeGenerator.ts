/**
 * DockerComposeGenerator — generates a per-bounded-context docker-compose file
 * containing the DBMS service (and related volume/network declarations) required
 * by the bounded context.
 *
 * Output: docker/{contextNameLower}-docker-compose.generated.yml
 *
 * The DBMS image, ports, env vars, and healthcheck are driven by:
 *   blueprint.boundedContext.dataService.dataAccess.managementSystem (default: 'postgres')
 *
 * The network name is driven by:
 *   domainModel.docker.network (default: 'inventory-network')
 *
 * The database name and exposed port come from:
 *   boundedContext.database.name / boundedContext.database.port
 */

import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext } from '../models/DataModel';
import { DbmsRegistry } from '../utils/DbmsRegistry';
import * as path from 'path';

export class DockerComposeGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const contextName = model.boundedContext.name;
    const contextNameLower = contextName.toLowerCase();

    // Resolve database config from data model (instance-level details)
    const databaseName =
      model.boundedContext.database?.name ?? `${contextNameLower}_db`;
    const dbPort = model.boundedContext.database?.port ?? 5432;

    // Resolve DBMS config from blueprint (technology-level choice)
    const managementSystem =
      this.blueprint?.boundedContext.dataService.dataAccess.managementSystem ??
      'postgres';
    const dbmsConfig = DbmsRegistry.getDbmsConfig(managementSystem);

    // Compose service and volume names
    const dbmsServiceName = `${managementSystem}-${databaseName}`;
    const volumeName = `${managementSystem}_${databaseName}_data`;

    // Network from domain-level docker config
    const networkName = model.domain.docker?.network ?? 'inventory-network';

    const context = {
      contextName,
      databaseName,
      dbPort,
      dbmsServiceName,
      dbmsImage: dbmsConfig.image,
      dbmsContainerPort: dbmsConfig.containerPort,
      dbmsDataPath: dbmsConfig.dataPath,
      envVars: dbmsConfig.getEnvVars(databaseName),
      healthcheck: dbmsConfig.healthcheck,
      volumeName,
      networkName,
      generationStamp: this.metadata?.generationStamp ?? '',
      generatedAt: this.metadata?.generatedAt ?? '',
      sourceFile: this.metadata?.sourceFile ?? '',
    };

    const outputPath = path.join(
      'docker',
      `${contextNameLower}-docker-compose.generated.yml`
    );

    await this.writeRenderedTemplate(
      'docker/docker-compose.generated.yml.hbs',
      context,
      outputPath,
      true // always overwrite — file is fully derived from the data model
    );
  }
}
