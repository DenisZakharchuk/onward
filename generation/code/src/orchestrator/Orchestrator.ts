/**
 * Orchestrator coordinates all code generators
 */

import chalk from 'chalk';
import {
  DomainModel,
  BoundedContextGenerationContext,
  GenerationMetadata,
} from '../models/DataModel';
import { Blueprint } from '../models/Blueprint';
import { IResultWriter } from '../abstractions/IResultWriter';
import { IGeneratorExecutionContext } from '../abstractions/GeneratorADT';
import { IExecutionScheduler } from '../abstractions/IExecutionScheduler';
import { DataModelParser } from '../parser/DataModelParser';
import { GenerationStamp } from '../utils/GenerationStamp';
import { GeneratorRegistry, GeneratorRegistration } from './GeneratorRegistry';
import { GeneratorRegistrar } from './GeneratorRegistrar';
import { SequentialScheduler } from './SequentialScheduler';
import * as path from 'path';

export interface OrchestratorOptions {
  skipTests?: boolean;
  dryRun?: boolean;
  force?: boolean;
  sourceFile?: string;  // Name of source data model file
  baseNamespace?: string;  // Base namespace prefix (default: 'Inventorization')
  blueprint?: Blueprint;
  /** Controls how many bounded contexts are generated simultaneously. Default: sequential. */
  contextScheduler?: IExecutionScheduler;
  /** Controls how many generators within a single phase run simultaneously. Default: sequential. */
  generatorScheduler?: IExecutionScheduler;
}

export class Orchestrator {
  private options: OrchestratorOptions;
  private registry: GeneratorRegistry;
  private writer: IResultWriter;
  private readonly contextScheduler: IExecutionScheduler;
  private readonly generatorScheduler: IExecutionScheduler;

  constructor(writer: IResultWriter, options: OrchestratorOptions = {}) {
    this.writer = writer;
    this.registry = new GeneratorRegistry();
    this.contextScheduler = options.contextScheduler ?? new SequentialScheduler();
    this.generatorScheduler = options.generatorScheduler ?? new SequentialScheduler();
    this.options = {
      ...options,
      skipTests: options.skipTests ?? false,
      dryRun: options.dryRun ?? false,
      force: options.force ?? true,
      baseNamespace: options.baseNamespace ?? 'Inventorization',
    };
  }

  /**
   * Groups an execution plan (already phase-sorted) into batches by phase number.
   * Generators within the same phase have no declared dependencies on each other
   * and write to distinct output directories, so they are safe to run concurrently.
   */
  private static groupByPhase(plan: GeneratorRegistration[]): GeneratorRegistration[][] {
    const groups = new Map<number, GeneratorRegistration[]>();
    for (const registration of plan) {
      const phase = registration.generator.phase;
      if (!groups.has(phase)) groups.set(phase, []);
      groups.get(phase)!.push(registration);
    }
    // Return groups in ascending phase order (Map insertion order is preserved because
    // the plan is already sorted by phase).
    return Array.from(groups.values());
  }

  /**
   * Generate complete code for all bounded contexts in the domain model
   */
  async generate(domain: DomainModel): Promise<void> {
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

    // Build a flattened generation context per bounded context
    const parser = new DataModelParser();
    const generationContexts = parser.buildGenerationContexts(domain);

    console.log(chalk.blue(`  Bounded Contexts: ${generationContexts.length}\n`));

    await this.contextScheduler.run(
      generationContexts.map((ctx) => async () => {
        console.log(chalk.blue(`\n📦 Generating: ${chalk.yellow(ctx.boundedContext.name)}\n`));
        await this.generateBoundedContext(ctx, context);
      })
    );

    console.log(chalk.green('\n✅ All bounded contexts generated.\n'));
  }

  /**
   * Generate code for a single bounded context
   */
  private async generateBoundedContext(
    ctx: BoundedContextGenerationContext,
    context: IGeneratorExecutionContext
  ): Promise<void> {
    this.initializeGenerators();

    // Create project directories
    const projectPaths = this.getProjectPaths(ctx);

    if (!this.options.dryRun) {
      await this.ensureDirectories(projectPaths, ctx);
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

    const executionPlan = this.registry.resolveExecutionPlan(ctx, context, enabledSlots);
    const phaseGroups = Orchestrator.groupByPhase(executionPlan);

    for (const group of phaseGroups) {
      await this.generatorScheduler.run(
        group.map((registration) => async () => {
          const generatorName = registration.generator.id;
          console.log(chalk.cyan(`  ⚙️  Running ${generatorName}...`));

          if (!this.options.dryRun) {
            await registration.generator.generate(ctx, context);
          } else {
            console.log(chalk.gray(`     (dry run - skipped)`));
          }
        })
      );
    }

    // Print summary
    this.printSummary(ctx, projectPaths, context.metadata.generationStamp);
  }

  /**
   * Initialize all generators in dependency order
   */
  private initializeGenerators(): void {
    this.registry = new GeneratorRegistry();
    GeneratorRegistrar.registerAll(this.registry, {
      hasEnums: (ctx) => this.hasEnums(ctx),
      resolveDataLayerKind: () => this.resolveDataLayerKind(),
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
  private getProjectPaths(ctx: BoundedContextGenerationContext) {
    const contextName = ctx.boundedContext.name;
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
  private async ensureDirectories(projectPaths: Record<string, string>, ctx: BoundedContextGenerationContext): Promise<void> {
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
      } else if (key === 'common' && this.hasEnums(ctx)) {
        await this.writer.ensureDirectory(path.join(dirPath as string, 'Enums'));
      }
    }
  }

  /**
   * Check if context has enums
   */
  private hasEnums(ctx: BoundedContextGenerationContext): boolean {
    return ctx.enums !== undefined && ctx.enums.length > 0;
  }

  /**
   * Print generation summary
   */
  private printSummary(ctx: BoundedContextGenerationContext, projectPaths: Record<string, string>, generationStamp: string): void {
    console.log(chalk.green('\n✅ Generation Summary:\n'));

    console.log(chalk.blue('  Generation:'));
    console.log(`    Stamp: ${chalk.yellow(generationStamp)}`);
    console.log(`    BoundedContext: ${chalk.yellow(ctx.boundedContext.name)}`);

    const entityCount = ctx.entities.length;
    const junctionCount = ctx.entities.filter((e) => e.isJunction).length;
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
