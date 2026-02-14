import Handlebars from 'handlebars';
import { DataModel } from '../models/DataModel';
import { BaseGenerator } from './BaseGenerator';
import { FileManager } from '../utils/FileManager';
import path from 'path';

interface ProjectContext {
  projectName: string;
  baseNamespace: string;
  itemGroup: string;
  targetFramework: string;
}

interface GlobalUsingsContext {
  baseNamespace: string;
  namespace: string; // Full BoundedContext namespace (e.g., "Inventorization.Goods" or "CompanyX.Goods")
  projectType: 'meta' | 'common' | 'dto' | 'domain' | 'api' | 'tests';
}

/**
 * Generates .csproj files and project infrastructure (GlobalUsings.cs, etc.)
 */
export class ProjectGenerator extends BaseGenerator {
  private metaCsprojTemplate!: HandlebarsTemplateDelegate;
  private commonCsprojTemplate!: HandlebarsTemplateDelegate;
  private dtoCsprojTemplate!: HandlebarsTemplateDelegate;
  private domainCsprojTemplate!: HandlebarsTemplateDelegate;
  private apiCsprojTemplate!: HandlebarsTemplateDelegate;
  private testsCsprojTemplate!: HandlebarsTemplateDelegate;
  private globalUsingsTemplate!: HandlebarsTemplateDelegate;

  async generate(model: DataModel): Promise<void> {
    if (!this.writer || !this.metadata) {
      throw new Error('Writer and metadata must be set before generation');
    }

    // Load templates
    const templateDir = path.join(__dirname, '../../templates');
    
    this.metaCsprojTemplate = Handlebars.compile(
      await FileManager.readFile(path.join(templateDir, 'meta.csproj.hbs'))
    );
    this.commonCsprojTemplate = Handlebars.compile(
      await FileManager.readFile(path.join(templateDir, 'common.csproj.hbs'))
    );
    this.dtoCsprojTemplate = Handlebars.compile(
      await FileManager.readFile(path.join(templateDir, 'dto.csproj.hbs'))
    );
    this.domainCsprojTemplate = Handlebars.compile(
      await FileManager.readFile(path.join(templateDir, 'domain.csproj.hbs'))
    );
    this.apiCsprojTemplate = Handlebars.compile(
      await FileManager.readFile(path.join(templateDir, 'api.csproj.hbs'))
    );
    this.testsCsprojTemplate = Handlebars.compile(
      await FileManager.readFile(path.join(templateDir, 'tests.csproj.hbs'))
    );
    this.globalUsingsTemplate = Handlebars.compile(
      await FileManager.readFile(path.join(templateDir, 'global-usings.hbs'))
    );

    // Generate all projects
    const contextName = model.boundedContext.name;
    
    await this.generateProject('meta', contextName, this.metaCsprojTemplate, model);
    await this.generateProject('common', contextName, this.commonCsprojTemplate, model);
    await this.generateProject('dto', contextName, this.dtoCsprojTemplate, model);
    await this.generateProject('domain', contextName, this.domainCsprojTemplate, model);
    await this.generateProject('api', contextName, this.apiCsprojTemplate, model);
    await this.generateProject('tests', contextName, this.testsCsprojTemplate, model);
  }

  private async generateProject(
    projectType: 'meta' | 'common' | 'dto' | 'domain' | 'api' | 'tests',
    contextName: string,
    template: HandlebarsTemplateDelegate,
    model: DataModel
  ): Promise<void> {
    const projectSuffix = this.getProjectSuffix(projectType);
    const projectName = `${this.metadata!.baseNamespace}.${contextName}.${projectSuffix}`;
    const projectDir = projectName; // Writer handles the root output directory
    
    // Generate .csproj file
    const csprojContext = this.buildProjectContext(
      projectName,
      this.metadata!.baseNamespace,
      model.boundedContext.namespace, // Use namespace from model
      projectType
    );
    const csprojContent = template(csprojContext);
    await this.writer!.write(
      path.join(projectDir, `${projectName}.csproj`),
      csprojContent,
      { overwrite: true, createDirectories: true }
    );
    
    // Generate GlobalUsings.cs
    const globalUsingsContext: GlobalUsingsContext = {
      baseNamespace: this.metadata!.baseNamespace,
      namespace: model.boundedContext.namespace,
      projectType
    };
    const globalUsingsContent = this.globalUsingsTemplate(globalUsingsContext);
    await this.writer!.write(
      path.join(projectDir, 'GlobalUsings.cs'),
      globalUsingsContent,
      { overwrite: true, createDirectories: true }
    );
  }

  private getProjectSuffix(projectType: string): string {
    switch (projectType) {
      case 'meta': return 'Meta';
      case 'common': return 'Common';
      case 'dto': return 'DTO';
      case 'domain': return 'Domain';
      case 'api': return 'API';
      case 'tests': return 'API.Tests';
      default: throw new Error(`Unknown project type: ${projectType}`);
    }
  }

  private buildProjectContext(
    projectName: string,
    baseNamespace: string,
    namespace: string, // Full namespace from BoundedContext (e.g., "Inventorization.Goods" or "CompanyX.Goods")
    projectType: string
  ): ProjectContext {
    return {
      projectName,
      baseNamespace,
      itemGroup: this.buildItemGroup(projectType, namespace, baseNamespace),
      targetFramework: 'net8.0'
    };
  }

  private buildItemGroup(projectType: string, namespace: string, baseNamespace: string): string {
    switch (projectType) {
      case 'meta':
        return `
  <ItemGroup>
    <ProjectReference Include="../${baseNamespace}.Base/${baseNamespace}.Base.csproj" />
  </ItemGroup>`;
      
      case 'common':
        return `
  <ItemGroup>
    <ProjectReference Include="../${baseNamespace}.Base/${baseNamespace}.Base.csproj" />
  </ItemGroup>`;
      
      case 'dto':
        return `
  <ItemGroup>
    <ProjectReference Include="../${baseNamespace}.Base/${baseNamespace}.Base.csproj" />
    <ProjectReference Include="../${namespace}.Meta/${namespace}.Meta.csproj" />
    <ProjectReference Include="../${namespace}.Common/${namespace}.Common.csproj" />
  </ItemGroup>`;
      
      case 'domain':
        return `
  <ItemGroup>
    <ProjectReference Include="../${baseNamespace}.Base/${baseNamespace}.Base.csproj" />
    <ProjectReference Include="../${namespace}.Meta/${namespace}.Meta.csproj" />
    <ProjectReference Include="../${namespace}.Common/${namespace}.Common.csproj" />
    <ProjectReference Include="../${namespace}.DTO/${namespace}.DTO.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
  </ItemGroup>`;
      
      case 'api':
        return `
  <ItemGroup>
    <ProjectReference Include="../${baseNamespace}.Base/${baseNamespace}.Base.csproj" />
    <ProjectReference Include="../${namespace}.Meta/${namespace}.Meta.csproj" />
    <ProjectReference Include="../${namespace}.Common/${namespace}.Common.csproj" />
    <ProjectReference Include="../${namespace}.DTO/${namespace}.DTO.csproj" />
    <ProjectReference Include="../${namespace}.Domain/${namespace}.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>`;
      
      case 'tests':
        return `
  <ItemGroup>
    <ProjectReference Include="../${namespace}.API/${namespace}.API.csproj" />
    <ProjectReference Include="../${namespace}.Domain/${namespace}.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
  </ItemGroup>`;
      
      default:
        return '';
    }
  }
}
