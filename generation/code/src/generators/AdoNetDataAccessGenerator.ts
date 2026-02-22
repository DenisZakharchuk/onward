import * as path from 'path';
import { BaseGenerator } from './BaseGenerator';
import { BoundedContextGenerationContext } from '../models/DataModel';

export class AdoNetDataAccessGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';
    const blProjectPath = `${baseNamespace}.${contextName}.BL`;

    await this.generateAdoNetRepository(blProjectPath, namespace, baseNamespace);
    await this.generateAdoNetUnitOfWork(blProjectPath, namespace, baseNamespace, contextName);
  }

  private async generateAdoNetRepository(
    blProjectPath: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const dataAccessDir = path.join(blProjectPath, 'DataAccess');
    const context = {
      namespace,
      baseNamespace,
    };

    const filePath = path.join(dataAccessDir, 'AdoNetRepository.cs');
    await this.writeRenderedTemplate(
      ['bl/repository/ado-net.generated.cs.hbs', 'ado-net-repository.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }

  private async generateAdoNetUnitOfWork(
    blProjectPath: string,
    namespace: string,
    baseNamespace: string,
    contextName: string
  ): Promise<void> {
    const dataAccessDir = path.join(blProjectPath, 'DataAccess');
    const context = {
      namespace,
      baseNamespace,
      contextName,
    };

    const filePath = path.join(dataAccessDir, `${contextName}AdoNetUnitOfWork.cs`);
    await this.writeRenderedTemplate(
      ['bl/unit-of-work/ado-net.generated.cs.hbs', 'ado-net-unit-of-work.generated.cs.hbs'],
      context,
      filePath,
      true
    );
  }
}