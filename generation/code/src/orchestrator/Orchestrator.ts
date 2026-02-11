/**
 * Orchestrator coordinates all code generators
 */

import chalk from 'chalk';
import { DataModel, GenerationMetadata } from '../models/DataModel';
import { IGenerator } from '../abstractions/IGenerator';
import { IResultWriter } from '../abstractions/IResultWriter';
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
import { GenerationStamp } from '../utils/GenerationStamp';
import * as path from 'path';

export interface OrchestratorOptions {
  skipTests?: boolean;
  dryRun?: boolean;
  force?: boolean;
  sourceFile?: string;  // Name of source data model file
  baseNamespace?: string;  // Base namespace prefix (default: 'Inventorization')
}

export class Orchestrator {
  private options: OrchestratorOptions;
  private generators: IGenerator[] = [];
  private writer: IResultWriter;

  constructor(writer: IResultWriter, options: OrchestratorOptions = {}) {
    this.writer = writer;
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

    console.log(chalk.gray(`  Generation Stamp: ${generationStamp}`));
    console.log(chalk.gray(`  Generated At: ${generatedAt}`));
    console.log(chalk.gray(`  Source: ${sourceFile}\n`));

    // Initialize generators (in dependency order)
    this.initializeGenerators();

    // Inject metadata and writer into all generators
    for (const generator of this.generators) {
      generator.setMetadata(metadata);
      generator.setWriter(this.writer);
    }

    // Create project directories
    const projectPaths = this.getProjectPaths(model);

    if (!this.options.dryRun) {
      await this.ensureDirectories(projectPaths, model);
    }

    // Run generators in order
    for (const generator of this.generators) {
      const generatorName = generator.constructor.name;
      console.log(chalk.cyan(`  ⚙️  Running ${generatorName}...`));

      if (!this.options.dryRun) {
        await generator.generate(model);
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
    // Order matters - dependencies first
    this.generators = [
      // Phase 1: Metadata (first - needed by other generators)
      new MetadataGenerator(),

      // Phase 2: Enums and base types
      // new EnumGenerator(),

      // Phase 3: Entities
      new EntityGenerator(),

      // Phase 4: DTOs
      new DtoGenerator(),

      // Phase 5: Configurations
      new ConfigurationGenerator(),

      // Phase 6: DbContext and UnitOfWork
      new DataAccessGenerator(),

      // Phase 7: Abstractions (Creators, Modifiers, Mappers, SearchProviders)
      new AbstractionGenerator(),

      // Phase 8: Validators
      new ValidatorGenerator(),

      // Phase 9: DataServices
      new ServiceGenerator(),

      // Phase 10: Controllers
      new ControllerGenerator(),

      // Phase 11: Tests (if not skipped)
      // if (!this.options.skipTests) {
      //   this.generators.push(new TestGenerator());
      // }

      // Phase 12: Project files, GlobalUsings.cs, and .csproj files (LAST)
      new ProjectGenerator(),
    ];
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
      domain: `${baseNamespace}.${contextName}.Domain`,
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
      if (key === 'domain') {
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Entities'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'EntityConfigurations'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Creators'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Modifiers'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Mappers'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'SearchProviders'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Validators'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'DataServices'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'DbContexts'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'DataAccess'));
        await this.writer.ensureDirectory(path.join(dirPath as string, 'PropertyAccessors'));
      } else if (key === 'api') {
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Controllers'));
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
    console.log(`    DTOs: ${chalk.yellow('5')} (Create, Update, Delete, Details, Search)`);
    console.log(`    Entity: ${chalk.yellow('1')} (Entity.cs)`);
    console.log(`    Total per entity: ${chalk.yellow('~15-20 files')} (when complete)`);

    console.log(chalk.blue('\n  Project Paths:'));
    Object.entries(projectPaths).forEach(([key, dirPath]) => {
      console.log(`    ${key}: ${chalk.gray(dirPath as string)}`);
    });
  }
}
