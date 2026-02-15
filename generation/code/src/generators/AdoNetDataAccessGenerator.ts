import * as path from 'path';
import { BaseGenerator } from './BaseGenerator';
import { DataModel } from '../models/DataModel';

export class AdoNetDataAccessGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';
    const domainProjectPath = `${baseNamespace}.${contextName}.Domain`;

    await this.generateAdoNetRepository(domainProjectPath, namespace, baseNamespace);
    await this.generateAdoNetUnitOfWork(domainProjectPath, namespace, baseNamespace, contextName);
  }

  private async generateAdoNetRepository(
    domainProjectPath: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const dataAccessDir = path.join(domainProjectPath, 'DataAccess');
    const context = {
      namespace,
      baseNamespace,
    };

    const filePath = path.join(dataAccessDir, 'AdoNetRepository.cs');
    await this.writeRenderedTemplate(
      ['domain/repository/ado-net.generated.cs.hbs', 'ado-net-repository.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }

  private async generateAdoNetUnitOfWork(
    domainProjectPath: string,
    namespace: string,
    baseNamespace: string,
    contextName: string
  ): Promise<void> {
    const dataAccessDir = path.join(domainProjectPath, 'DataAccess');
    const context = {
      namespace,
      baseNamespace,
      contextName,
    };

    const filePath = path.join(dataAccessDir, `${contextName}AdoNetUnitOfWork.cs`);
    await this.writeRenderedTemplate(
      ['domain/unit-of-work/ado-net.generated.cs.hbs', 'ado-net-unit-of-work.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }
}