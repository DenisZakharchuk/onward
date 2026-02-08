/**
 * Orchestrator coordinates all code generators
 */

import chalk from 'chalk';
import { DataModel } from '../models/DataModel';
import { BaseGenerator } from '../generators/BaseGenerator';
import { DtoGenerator } from '../generators/DtoGenerator';
import { EntityGenerator } from '../generators/EntityGenerator';
import * as path from 'path';

export interface OrchestratorOptions {
  skipTests?: boolean;
  dryRun?: boolean;
  force?: boolean;
}

export class Orchestrator {
  private options: OrchestratorOptions;
  private generators: BaseGenerator[] = [];

  constructor(options: OrchestratorOptions = {}) {
    this.options = {
      skipTests: false,
      dryRun: false,
      force: true,
      ...options,
    };
  }

  /**
   * Generate complete BoundedContext code
   */
  async generate(model: DataModel, outputDir: string): Promise<void> {
    console.log(chalk.blue('\n🚀 Starting code generation...\n'));

    // Initialize generators (in dependency order)
    this.initializeGenerators();

    // Create project directories
    const projectPaths = this.getProjectPaths(model, outputDir);

    if (!this.options.dryRun) {
      await this.ensureDirectories(projectPaths);
    }

    // Run generators in order
    for (const generator of this.generators) {
      const generatorName = generator.constructor.name;
      console.log(chalk.cyan(`  ⚙️  Running ${generatorName}...`));

      if (!this.options.dryRun) {
        await generator.generate(model, outputDir);
      } else {
        console.log(chalk.gray(`     (dry run - skipped)`));
      }
    }

    // Print summary
    this.printSummary(model, projectPaths);
  }

  /**
   * Initialize all generators in dependency order
   */
  private initializeGenerators(): void {
    // Order matters - dependencies first
    this.generators = [
      // Phase 1: Enums and base types
      // new EnumGenerator(),

      // Phase 2: Entities
      new EntityGenerator(),

      // Phase 3: DTOs
      new DtoGenerator(),

      // Phase 4: Configurations
      // new ConfigurationGenerator(),

      // Phase 5: DbContext and UnitOfWork
      // new DataAccessGenerator(),

      // Phase 6: Abstractions (Creators, Modifiers, Mappers, SearchProviders)
      // new AbstractionGenerator(),

      // Phase 7: Validators
      // new ValidatorGenerator(),

      // Phase 8: DataServices
      // new ServiceGenerator(),

      // Phase 9: Controllers
      // new ControllerGenerator(),

      // Phase 10: Tests (if not skipped)
      // if (!this.options.skipTests) {
      //   this.generators.push(new TestGenerator());
      // }

      // Phase 11: Project files and DI registrations
      // new ProjectFileGenerator(),
      // new ProgramGenerator(),
    ];
  }

  /**
   * Get project directory paths
   */
  private getProjectPaths(model: DataModel, outputDir: string) {
    const contextName = model.boundedContext.name;

    return {
      common: path.join(outputDir, `Inventorization.${contextName}.Common`),
      dto: path.join(outputDir, `Inventorization.${contextName}.DTO`),
      domain: path.join(outputDir, `Inventorization.${contextName}.Domain`),
      api: path.join(outputDir, `Inventorization.${contextName}.API`),
      tests: path.join(outputDir, `Inventorization.${contextName}.API.Tests`),
    };
  }

  /**
   * Ensure all project directories exist
   */
  private async ensureDirectories(projectPaths: any): Promise<void> {
    const fs = await import('fs-extra');

    for (const [key, dirPath] of Object.entries(projectPaths)) {
      await fs.ensureDir(dirPath as string);

      // Create subdirectories
      if (key === 'domain') {
        await fs.ensureDir(path.join(dirPath as string, 'Entities'));
        await fs.ensureDir(path.join(dirPath as string, 'EntityConfigurations'));
        await fs.ensureDir(path.join(dirPath as string, 'Creators'));
        await fs.ensureDir(path.join(dirPath as string, 'Modifiers'));
        await fs.ensureDir(path.join(dirPath as string, 'Mappers'));
        await fs.ensureDir(path.join(dirPath as string, 'SearchProviders'));
        await fs.ensureDir(path.join(dirPath as string, 'Validators'));
        await fs.ensureDir(path.join(dirPath as string, 'DataServices'));
        await fs.ensureDir(path.join(dirPath as string, 'DbContexts'));
        await fs.ensureDir(path.join(dirPath as string, 'DataAccess'));
        await fs.ensureDir(path.join(dirPath as string, 'PropertyAccessors'));
      } else if (key === 'api') {
        await fs.ensureDir(path.join(dirPath as string, 'Controllers'));
      } else if (key === 'common' && this.hasEnums(projectPaths)) {
        await fs.ensureDir(path.join(dirPath as string, 'Enums'));
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
  private printSummary(model: DataModel, projectPaths: any): void {
    console.log(chalk.green('\n✅ Generation Summary:\n'));

    const entityCount = model.entities.length;
    const junctionCount = model.entities.filter((e) => e.isJunction).length;
    const regularCount = entityCount - junctionCount;

    console.log(chalk.blue('  Entities:'));
    console.log(`    Regular Entities: ${chalk.yellow(regularCount)}`);
    console.log(`    Junction Entities: ${chalk.yellow(junctionCount)}`);
    console.log(`    Total: ${chalk.yellow(entityCount)}`);

    console.log(chalk.blue('\n  Generated Files per Entity:'));
    console.log(`    DTOs: ${chalk.yellow('5')} (Create, Update, Delete, Details, Search)`);
    console.log(`    Domain: ${chalk.yellow('2')} (Entity.generated.cs, Entity.cs)`);
    console.log(`    Total per entity: ${chalk.yellow('~15-20 files')}`);

    console.log(chalk.blue('\n  Project Paths:'));
    Object.entries(projectPaths).forEach(([key, dirPath]) => {
      console.log(`    ${key}: ${chalk.gray(dirPath as string)}`);
    });
  }
}
