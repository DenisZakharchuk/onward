/**
 * Orchestrator coordinates all code generators
 */

import chalk from 'chalk';
import { DataModel, GenerationMetadata } from '../models/DataModel';
import { Blueprint } from '../models/Blueprint';
import { IResultWriter } from '../abstractions/IResultWriter';
import { IGeneratorExecutionContext } from '../abstractions/GeneratorADT';
import { DtoGenerator } from '../generators/DtoGenerator';
import { EntityGenerator } from '../generators/EntityGenerator';
import { ConfigurationGenerator } from '../generators/ConfigurationGenerator';
import { AbstractionGenerator } from '../generators/AbstractionGenerator';
import { ValidatorGenerator } from '../generators/ValidatorGenerator';
import { ServiceGenerator } from '../generators/ServiceGenerator';
import { DataAccessGenerator } from '../generators/DataAccessGenerator';
import { ControllerGenerator } from '../generators/ControllerGenerator';
import { MetadataGenerator } from '../generators/MetadataGenerator';
import { ProjectGenerator } from '../generators/ProjectGenerator';
import { EnumGenerator } from '../generators/EnumGenerator';
import { QueryBuilderGenerator } from '../generators/QueryBuilderGenerator';
import { SearchServiceGenerator } from '../generators/SearchServiceGenerator';
import { QueryControllerGenerator } from '../generators/QueryControllerGenerator';
import { ProjectionMapperInterfaceGenerator } from '../generators/ProjectionMapperInterfaceGenerator';
import { ProjectionMapperGenerator } from '../generators/ProjectionMapperGenerator';
import { ProjectionDtoGenerator } from '../generators/ProjectionDtoGenerator';
import { SearchFieldsGenerator } from '../generators/SearchFieldsGenerator';
import { SearchQueryValidatorGenerator } from '../generators/SearchQueryValidatorGenerator';
import { GenerationStamp } from '../utils/GenerationStamp';
import * as path from 'path';
import { DiGenerator } from '../generators/DiGenerator';
import { ApiProgramGenerator } from '../generators/ApiProgramGenerator';
import { TestGenerator } from '../generators/TestGenerator';
import { GeneratorRegistry } from './GeneratorRegistry';
import { LegacyGeneratorAdapter, LegacyGeneratorDescriptor } from './LegacyGeneratorAdapter';
import { MinimalApiProgramGenerator } from '../generators/MinimalApiProgramGenerator';
import { MinimalApiEndpointsGenerator } from '../generators/MinimalApiEndpointsGenerator';
import { AdoNetDataAccessGenerator } from '../generators/AdoNetDataAccessGenerator';
import { AdoNetDiGenerator } from '../generators/AdoNetDiGenerator';
import { AdoNetApiProgramGenerator } from '../generators/AdoNetApiProgramGenerator';
import { AdoNetMinimalApiProgramGenerator } from '../generators/AdoNetMinimalApiProgramGenerator';

export interface OrchestratorOptions {
  skipTests?: boolean;
  dryRun?: boolean;
  force?: boolean;
  sourceFile?: string;  // Name of source data model file
  baseNamespace?: string;  // Base namespace prefix (default: 'Inventorization')
  blueprint?: Blueprint;
}

export class Orchestrator {
  private options: OrchestratorOptions;
  private registry: GeneratorRegistry;
  private writer: IResultWriter;

  constructor(writer: IResultWriter, options: OrchestratorOptions = {}) {
    this.writer = writer;
    this.registry = new GeneratorRegistry();
    this.options = {
      ...options,
      skipTests: options.skipTests ?? false,
      dryRun: options.dryRun ?? false,
      force: options.force ?? true,
      baseNamespace: options.baseNamespace ?? 'Inventorization',
    };
  }

  /**
   * Generate complete BoundedContext code
   */
  async generate(model: DataModel): Promise<void> {
    console.log(chalk.blue('\n🚀 Starting code generation...\n'));

    // Generate unique stamp for this generation run
    const generationStamp = GenerationStamp.create();
    const generatedAt = GenerationStamp.getTimestamp();
    const sourceFile = this.options.sourceFile || 'unknown';

    const metadata: GenerationMetadata = {
      generationStamp,
      generatedAt,
      sourceFile,
      baseNamespace: this.options.baseNamespace!,
    };

    const context: IGeneratorExecutionContext = {
      metadata,
      writer: this.writer,
      blueprint: this.options.blueprint,
      options: {
        dryRun: this.options.dryRun!,
        force: this.options.force!,
        skipTests: this.options.skipTests!,
      },
    };

    console.log(chalk.gray(`  Generation Stamp: ${generationStamp}`));
    console.log(chalk.gray(`  Generated At: ${generatedAt}`));
    console.log(chalk.gray(`  Source: ${sourceFile}\n`));

    this.initializeGenerators();

    // Create project directories
    const projectPaths = this.getProjectPaths(model);

    if (!this.options.dryRun) {
      await this.ensureDirectories(projectPaths, model);
    }

    const enabledSlots = new Set<string>(['core']);
    const presentationKind = this.resolvePresentationKind();
    const dataLayerKind = this.resolveDataLayerKind();
    enabledSlots.add(presentationKind);
    enabledSlots.add(dataLayerKind);

    if (presentationKind === 'grpc') {
      throw new Error('Blueprint presentation kind "grpc" is not implemented yet.');
    }

    if (!this.options.skipTests) {
      enabledSlots.add('tests');
    }

    const executionPlan = this.registry.resolveExecutionPlan(model, context, enabledSlots);

    for (const registration of executionPlan) {
      const generatorName = registration.generator.id;
      console.log(chalk.cyan(`  ⚙️  Running ${generatorName}...`));

      if (!this.options.dryRun) {
        await registration.generator.generate(model, context);
      } else {
        console.log(chalk.gray(`     (dry run - skipped)`));
      }
    }

    // Print summary
    this.printSummary(model, projectPaths, generationStamp);
  }

  /**
   * Initialize all generators in dependency order
   */
  private initializeGenerators(): void {
    this.registry = new GeneratorRegistry();

    this.registerLegacyGenerator(new MetadataGenerator(), {
      id: 'MetadataGenerator',
      domain: 'metadata',
      phase: 1,
      ambiguity: 'composite',
      provides: ['metadata'],
    });

    this.registerLegacyGenerator(new EnumGenerator(), {
      id: 'EnumGenerator',
      domain: 'metadata',
      phase: 2,
      ambiguity: 'optional',
      requires: ['metadata'],
      provides: ['enums'],
      applies: (model) => this.hasEnums(model),
    });

    this.registerLegacyGenerator(new EntityGenerator(), {
      id: 'EntityGenerator',
      domain: 'bl',
      phase: 3,
      ambiguity: 'variant',
      requires: ['metadata'],
      provides: ['entities'],
      dependsOn: ['MetadataGenerator'],
    });

    this.registerLegacyGenerator(new DtoGenerator(), {
      id: 'DtoGenerator',
      domain: 'dto',
      phase: 4,
      ambiguity: 'composite',
      requires: ['entities'],
      provides: ['dto'],
      dependsOn: ['EntityGenerator'],
    });

    this.registerLegacyGenerator(new ProjectionDtoGenerator(), {
      id: 'ProjectionDtoGenerator',
      domain: 'dto',
      phase: 4,
      ambiguity: 'variant',
      requires: ['entities'],
      provides: ['projection-dto'],
      dependsOn: ['EntityGenerator'],
    });

    this.registerLegacyGenerator(new ConfigurationGenerator(), {
      id: 'ConfigurationGenerator',
      domain: 'bl',
      phase: 5,
      ambiguity: 'variant',
      requires: ['entities'],
      provides: ['entity-configurations'],
      dependsOn: ['EntityGenerator'],
      optionalSlot: 'ef-core',
    });

    this.registerLegacyGenerator(new DataAccessGenerator(), {
      id: 'DataAccessGenerator',
      domain: 'bl',
      phase: 6,
      ambiguity: 'composite',
      requires: ['entity-configurations'],
      provides: ['data-access'],
      dependsOn: ['ConfigurationGenerator'],
      optionalSlot: 'ef-core',
    });

    this.registerLegacyGenerator(new AdoNetDataAccessGenerator(), {
      id: 'AdoNetDataAccessGenerator',
      domain: 'bl',
      phase: 6,
      ambiguity: 'composite',
      requires: ['entities'],
      provides: ['data-access'],
      dependsOn: ['EntityGenerator'],
      optionalSlot: 'ado-net',
    });

    this.registerLegacyGenerator(new AbstractionGenerator(), {
      id: 'AbstractionGenerator',
      domain: 'bl',
      phase: 7,
      ambiguity: 'composite',
      requires: ['entities'],
      provides: ['abstractions'],
      dependsOn: ['EntityGenerator'],
    });

    this.registerLegacyGenerator(new QueryBuilderGenerator(), {
      id: 'QueryBuilderGenerator',
      domain: 'query',
      phase: 7,
      ambiguity: 'deterministic',
      requires: ['entities'],
      provides: ['query-builder'],
      dependsOn: ['EntityGenerator'],
    });

    this.registerLegacyGenerator(new ProjectionMapperInterfaceGenerator(), {
      id: 'ProjectionMapperInterfaceGenerator',
      domain: 'query',
      phase: 7,
      ambiguity: 'deterministic',
      requires: ['projection-dto'],
      provides: ['projection-mapper-interface'],
      dependsOn: ['ProjectionDtoGenerator'],
    });

    this.registerLegacyGenerator(new ProjectionMapperGenerator(), {
      id: 'ProjectionMapperGenerator',
      domain: 'query',
      phase: 7,
      ambiguity: 'variant',
      requires: ['projection-mapper-interface'],
      provides: ['projection-mapper'],
      dependsOn: ['ProjectionMapperInterfaceGenerator'],
    });

    this.registerLegacyGenerator(new SearchFieldsGenerator(), {
      id: 'SearchFieldsGenerator',
      domain: 'query',
      phase: 7,
      ambiguity: 'variant',
      requires: ['entities'],
      provides: ['search-fields'],
      dependsOn: ['EntityGenerator'],
    });

    this.registerLegacyGenerator(new ValidatorGenerator(), {
      id: 'ValidatorGenerator',
      domain: 'bl',
      phase: 8,
      ambiguity: 'composite',
      requires: ['dto'],
      provides: ['validators'],
      dependsOn: ['DtoGenerator'],
    });

    this.registerLegacyGenerator(new SearchQueryValidatorGenerator(), {
      id: 'SearchQueryValidatorGenerator',
      domain: 'query',
      phase: 8,
      ambiguity: 'deterministic',
      requires: ['search-fields'],
      provides: ['search-query-validator'],
      dependsOn: ['SearchFieldsGenerator'],
    });

    this.registerLegacyGenerator(new ServiceGenerator(), {
      id: 'ServiceGenerator',
      domain: 'bl',
      phase: 9,
      ambiguity: 'deterministic',
      requires: ['validators'],
      provides: ['services'],
      dependsOn: ['ValidatorGenerator'],
    });

    this.registerLegacyGenerator(new SearchServiceGenerator(), {
      id: 'SearchServiceGenerator',
      domain: 'query',
      phase: 9,
      ambiguity: 'deterministic',
      requires: ['query-builder', 'projection-mapper'],
      provides: ['search-services'],
      dependsOn: ['QueryBuilderGenerator', 'ProjectionMapperGenerator'],
    });

    this.registerLegacyGenerator(new ControllerGenerator(), {
      id: 'ControllerGenerator',
      domain: 'api',
      phase: 10,
      ambiguity: 'deterministic',
      requires: ['services'],
      provides: ['controllers'],
      dependsOn: ['ServiceGenerator'],
      optionalSlot: 'controllers',
    });

    this.registerLegacyGenerator(new QueryControllerGenerator(), {
      id: 'QueryControllerGenerator',
      domain: 'api',
      phase: 10,
      ambiguity: 'variant',
      requires: ['search-services'],
      provides: ['query-controllers'],
      dependsOn: ['SearchServiceGenerator'],
      optionalSlot: 'controllers',
    });

    this.registerLegacyGenerator(new MinimalApiEndpointsGenerator(), {
      id: 'MinimalApiEndpointsGenerator',
      domain: 'api',
      phase: 10,
      ambiguity: 'deterministic',
      requires: ['services'],
      provides: ['minimal-api-endpoints'],
      dependsOn: ['ServiceGenerator'],
      optionalSlot: 'minimal-api',
    });

    this.registerLegacyGenerator(new DiGenerator(), {
      id: 'DiGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'composite',
      requires: ['services', 'search-services'],
      provides: ['di'],
      dependsOn: ['ServiceGenerator', 'SearchServiceGenerator'],
      optionalSlot: 'ef-core',
    });

    this.registerLegacyGenerator(new AdoNetDiGenerator(), {
      id: 'AdoNetDiGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'composite',
      requires: ['services', 'search-services', 'data-access'],
      provides: ['di'],
      dependsOn: ['ServiceGenerator', 'SearchServiceGenerator', 'AdoNetDataAccessGenerator'],
      optionalSlot: 'ado-net',
    });

    this.registerLegacyGenerator(new ApiProgramGenerator(), {
      id: 'ApiProgramGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'deterministic',
      requires: ['di'],
      provides: ['api-program'],
      dependsOn: ['DiGenerator'],
      optionalSlot: 'controllers',
      applies: () => this.resolveDataLayerKind() === 'ef-core',
    });

    this.registerLegacyGenerator(new AdoNetApiProgramGenerator(), {
      id: 'AdoNetApiProgramGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'deterministic',
      requires: ['di'],
      provides: ['api-program'],
      dependsOn: ['AdoNetDiGenerator'],
      optionalSlot: 'controllers',
      applies: () => this.resolveDataLayerKind() === 'ado-net',
    });

    this.registerLegacyGenerator(new MinimalApiProgramGenerator(), {
      id: 'MinimalApiProgramGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'deterministic',
      requires: ['di', 'minimal-api-endpoints'],
      provides: ['api-program'],
      dependsOn: ['DiGenerator', 'MinimalApiEndpointsGenerator'],
      optionalSlot: 'minimal-api',
      applies: () => this.resolveDataLayerKind() === 'ef-core',
    });

    this.registerLegacyGenerator(new AdoNetMinimalApiProgramGenerator(), {
      id: 'AdoNetMinimalApiProgramGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'deterministic',
      requires: ['di', 'minimal-api-endpoints'],
      provides: ['api-program'],
      dependsOn: ['AdoNetDiGenerator', 'MinimalApiEndpointsGenerator'],
      optionalSlot: 'minimal-api',
      applies: () => this.resolveDataLayerKind() === 'ado-net',
    });

    this.registerLegacyGenerator(new TestGenerator(), {
      id: 'TestGenerator',
      domain: 'tests',
      phase: 12,
      ambiguity: 'optional',
      requires: ['services', 'validators', 'projection-mapper'],
      provides: ['tests'],
      dependsOn: ['ServiceGenerator', 'ValidatorGenerator', 'ProjectionMapperGenerator'],
      optionalSlot: 'tests',
    });

    this.registerLegacyGenerator(new ProjectGenerator(), {
      id: 'ProjectGenerator',
      domain: 'project',
      phase: 13,
      ambiguity: 'variant',
      requires: ['di', 'api-program'],
      provides: ['projects'],
    });
  }

  private registerLegacyGenerator(
    generator: {
      setMetadata(metadata: GenerationMetadata): void;
      setWriter(writer: IResultWriter): void;
      generate(model: DataModel): Promise<void>;
    },
    descriptor: LegacyGeneratorDescriptor & {
      dependsOn?: readonly string[];
      optionalSlot?: string;
    }
  ): void {
    const { dependsOn, optionalSlot, ...adapterDescriptor } = descriptor;

    this.registry.register({
      generator: new LegacyGeneratorAdapter(generator, adapterDescriptor),
      dependsOn,
      optionalSlot,
    });
  }

  private resolvePresentationKind(): 'controllers' | 'minimal-api' | 'grpc' {
    return this.options.blueprint?.boundedContext.presentation.kind || 'controllers';
  }

  private resolveDataLayerKind(): 'ef-core' | 'ado-net' {
    const dataAccess = this.options.blueprint?.boundedContext.dataService.dataAccess;

    if (dataAccess && 'ado' in dataAccess) {
      return 'ado-net';
    }

    return 'ef-core';
  }

  /**
   * Get project directory paths (relative to output directory)
   */
  private getProjectPaths(model: DataModel) {
    const contextName = model.boundedContext.name;
    const baseNamespace = this.options.baseNamespace!;

    return {
      meta: `${baseNamespace}.${contextName}.Meta`,
      common: `${baseNamespace}.${contextName}.Common`,
      dto: `${baseNamespace}.${contextName}.DTO`,
      bl: `${baseNamespace}.${contextName}.BL`,
      di: `${baseNamespace}.${contextName}.DI`,
      api: `${baseNamespace}.${contextName}.API`,
      tests: `${baseNamespace}.${contextName}.API.Tests`,
    };
  }

  /**
   * Ensure all project directories exist
   */
  private async ensureDirectories(projectPaths: Record<string, string>, model: DataModel): Promise<void> {
    for (const [key, dirPath] of Object.entries(projectPaths)) {
      await this.writer.ensureDirectory(dirPath as string);

      // Create subdirectories
      if (key === 'bl') {
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Entities'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'EntityConfigurations'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Creators'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Modifiers'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Mappers'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Mappers/Projection'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'SearchProviders'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Validators'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'DataServices'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Services/Query'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'QueryBuilders'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Constants'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'DbContexts'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'DataAccess'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'PropertyAccessors'));
      } else if (key === 'dto') {
        await this.writer.ensureDirectory(path.join(dirPath as string, 'DTO'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'ADTs'));
      } else if (key === 'api') {
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Controllers'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Controllers/Query'));
      } else if (key === 'di') {
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Extensions'));
      } else if (key === 'tests') {
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Services'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Validators'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Mappers'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Instantiation'));
      } else if (key === 'common' && this.hasEnums(model)) {
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Enums'));
      }
    }
  }

  /**
   * Check if model has enums
   */
  private hasEnums(model: DataModel): boolean {
    return model.enums !== undefined && model.enums.length > 0;
  }

  /**
   * Print generation summary
   */
  private printSummary(model: DataModel, projectPaths: Record<string, string>, generationStamp: string): void {
    console.log(chalk.green('\n✅ Generation Summary:\n'));

    console.log(chalk.blue('  Generation:'));
    console.log(`    Stamp: ${chalk.yellow(generationStamp)}`);
    console.log(`    BoundedContext: ${chalk.yellow(model.boundedContext.name)}`);

    const entityCount = model.entities.length;
    const junctionCount = model.entities.filter((e) => e.isJunction).length;
    const regularCount = entityCount - junctionCount;

    console.log(chalk.blue('\n  Entities:'));
    console.log(`    Regular Entities: ${chalk.yellow(regularCount)}`);
    console.log(`    Junction Entities: ${chalk.yellow(junctionCount)}`);
    console.log(`    Total: ${chalk.yellow(entityCount)}`);

    console.log(chalk.blue('\n  Generated Files per Entity:'));
    console.log(`    DTOs: ${chalk.yellow('6')} (Create, Update, Delete, Details, Search, Projection)`);
    console.log(`    Entity: ${chalk.yellow('1')} (Entity.cs)`);
    console.log(`    ADT Query Infrastructure: ${chalk.yellow('8')} (QueryBuilder, SearchService, QueryController, ProjectionMapper, etc.)`);
    console.log(`    Total per entity: ${chalk.yellow('~20-25 files')} (when complete)`);

    console.log(chalk.blue('\n  Project Paths:'));
    Object.entries(projectPaths).forEach(([key, dirPath]) => {
      console.log(`    ${key}: ${chalk.gray(dirPath as string)}`);
    });
  }
}
