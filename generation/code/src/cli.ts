#!/usr/bin/env node

/**
 * CLI entry point for the BoundedContext code generator
 * Composition root - creates and wires dependencies
 */

import yargs from 'yargs';
import { hideBin } from 'yargs/helpers';
import chalk from 'chalk';
import ora from 'ora';
import { FileModelProvider } from './providers/FileModelProvider';
import { FileBlueprintProvider } from './providers/FileBlueprintProvider';
import { Blueprint } from './models/Blueprint';
import { DomainModel } from './models/DataModel';
import { FileResultWriter } from './writers/FileResultWriter';
import { Orchestrator } from './orchestrator/Orchestrator';
import { SequentialScheduler } from './orchestrator/SequentialScheduler';
import { ConcurrentScheduler } from './orchestrator/ConcurrentScheduler';
import * as path from 'path';

interface GenerateOptions {
  outputDir: string;
  namespace?: string;
  baseNamespace?: string;
  blueprint?: string;
  skipTests: boolean;
  dryRun: boolean;
  force: boolean;
  /** Max number of tasks running simultaneously. Undefined = sequential (default). */
  concurrency?: number;
}

function resolveBlueprintDtoLayout(blueprint: Blueprint): 'class' | 'record' {
  return blueprint.boundedContext.dataService.dto;
}

function resolveBlueprintDataLayer(blueprint: Blueprint): 'ef-core' | 'ado-net' {
  return 'ado' in blueprint.boundedContext.dataService.dataAccess ? 'ado-net' : 'ef-core';
}

function validateBlueprintCompatibility(
  domain: DomainModel,
  blueprint?: Blueprint
): void {
  if (!blueprint) return;

  const blueprintDtoLayout = resolveBlueprintDtoLayout(blueprint);

  for (const ctx of domain.boundedContexts) {
    const modelDtoLayout = ctx.dtoLayout;
    if (modelDtoLayout && modelDtoLayout !== blueprintDtoLayout) {
      throw new Error(
        `Conflict detected for DTO layout in '${ctx.name}': data model has '${modelDtoLayout}', ` +
        `but blueprint requires '${blueprintDtoLayout}'.` +
        `\nConflict policy is fail-on-conflict.`
      );
    }
    // Apply resolved layout back to context
    ctx.dtoLayout = blueprintDtoLayout;
  }
}

async function generateCommand(dataModelPath: string, options: GenerateOptions) {
  const spinner = ora('Loading data model...').start();

  try {
    // Create dependencies (composition root)
    const modelProvider = new FileModelProvider();
    const blueprintProvider = new FileBlueprintProvider();
    const outputDir = path.resolve(options.outputDir);
    const resultWriter = new FileResultWriter(outputDir);

    // Load and validate data model
    spinner.text = 'Validating data model...';
    const model = await modelProvider.load(dataModelPath);

    let blueprint: Blueprint | undefined;
    if (options.blueprint) {
      spinner.text = 'Validating blueprint...';
      blueprint = await blueprintProvider.load(options.blueprint);
      validateBlueprintCompatibility(model, blueprint);
    }

    spinner.succeed(`Data model validated: ${chalk.green(model.boundedContexts.map(c => c.name).join(', '))}`);

    // Override namespace if provided — applies to each bounded context
    for (const ctx of model.boundedContexts) {
      if (options.namespace && model.boundedContexts.length === 1) {
        ctx.namespace = options.namespace;
      } else if (options.baseNamespace) {
        ctx.namespace = `${options.baseNamespace}.${ctx.name}`;
      }
    }

    console.log(chalk.blue('\nGeneration Settings:'));
    console.log(`  Bounded Contexts: ${chalk.yellow(model.boundedContexts.map(c => c.name).join(', '))}`);
    console.log(`  Base Namespace: ${chalk.yellow(options.baseNamespace || 'Inventorization')}`);
    if (options.blueprint) {
      console.log(`  Blueprint: ${chalk.yellow(path.resolve(options.blueprint))}`);
      console.log(`  DTO Layout (resolved): ${chalk.yellow(blueprint ? resolveBlueprintDtoLayout(blueprint) : 'class')}`);
      console.log(`  Presentation Kind: ${chalk.yellow(blueprint?.boundedContext.presentation.kind || 'controllers')}`);
      console.log(`  Data Layer: ${chalk.yellow(blueprint ? resolveBlueprintDataLayer(blueprint) : 'ef-core')}`);
    }
    console.log(`  Output Directory: ${chalk.yellow(outputDir)}`);
    console.log(`  Skip Tests: ${chalk.yellow(options.skipTests)}`);
    console.log(`  Dry Run: ${chalk.yellow(options.dryRun)}`);
    console.log(`  Force Overwrite: ${chalk.yellow(options.force)}`);
    console.log(
      `  Concurrency: ${chalk.yellow(options.concurrency !== undefined ? options.concurrency : 'sequential')}\n`
    );

    if (options.dryRun) {
      console.log(chalk.yellow('🔍 DRY RUN - No files will be written\n'));
    }

    // Generate code with dependency injection
    spinner.start('Generating code...');

    const scheduler = options.concurrency !== undefined
      ? new ConcurrentScheduler(options.concurrency)
      : new SequentialScheduler();

    const orchestrator = new Orchestrator(resultWriter, {
      skipTests: options.skipTests,
      dryRun: options.dryRun,
      force: options.force,
      sourceFile: path.basename(dataModelPath),
      baseNamespace: options.baseNamespace,
      blueprint,
      contextScheduler: scheduler,
      generatorScheduler: scheduler,
    });

    await orchestrator.generate(model);

    spinner.succeed(chalk.green('✅ Code generation completed successfully!'));

    if (!options.dryRun) {
      console.log(chalk.blue('\n📦 Next Steps:'));
      console.log(`  1. Review generated files in: ${chalk.yellow(outputDir)}`);
      console.log(`  2. Add custom business logic to ${chalk.yellow('domain services')}`);
      console.log(
        `  3. Build the solution: ${chalk.yellow('dotnet build')}`
      );
      console.log(
        `  4. Run migrations: ${chalk.yellow('dotnet ef migrations add InitialCreate')}`
      );
      console.log(`  5. Update docker-compose.yml with the new PostgreSQL service`);
    }
  } catch (error) {
    spinner.fail('Generation failed');
    console.error(chalk.red('\n❌ Error:'), error instanceof Error ? error.message : error);
    process.exit(1);
  }
}

async function validateCommand(dataModelPath: string, blueprintPath?: string) {
  const spinner = ora('Validating data model...').start();

  try {
    // Create model provider (composition root)
    const modelProvider = new FileModelProvider();
    const blueprintProvider = new FileBlueprintProvider();

    // Validate
    const result = await modelProvider.validate(dataModelPath);

    if (!result.valid) {
      spinner.fail('Validation failed');
      console.error(chalk.red('\n❌ Errors:'));
      result.errors.forEach((error) => console.error(chalk.red(`  - ${error}`)));
      process.exit(1);
    }

    let blueprint: Blueprint | undefined;
    if (blueprintPath) {
      spinner.text = 'Validating blueprint...';
      const blueprintResult = await blueprintProvider.validate(blueprintPath);

      if (!blueprintResult.valid) {
        spinner.fail('Validation failed');
        console.error(chalk.red('\n❌ Errors:'));
        blueprintResult.errors.forEach((error) => console.error(chalk.red(`  - ${error}`)));
        process.exit(1);
      }

      blueprint = blueprintResult.blueprint;
      validateBlueprintCompatibility(result.model!, blueprint);
    }

    const model = result.model!;
    spinner.succeed(chalk.green('✅ Data model is valid!'));

    console.log(chalk.blue('\nDomain Model Summary:'));
    console.log(`  Bounded Contexts: ${chalk.yellow(model.boundedContexts.length)}`);
    console.log(`  Shared Enums: ${chalk.yellow(model.enums?.length || 0)}`);

    model.boundedContexts.forEach((ctx) => {
      console.log(chalk.blue(`\n  [${ctx.name}]`));
      console.log(`    Namespace: ${chalk.yellow(ctx.namespace)}`);
      console.log(`    Entities: ${chalk.yellow(ctx.dataModel.entities.length)}`);
      console.log(`    Relationships: ${chalk.yellow(ctx.dataModel.relationships?.length || 0)}`);
      console.log(`    Context Enums: ${chalk.yellow(ctx.enums?.length || 0)}`);

      ctx.dataModel.entities.forEach((entity) => {
        console.log(
          `      - ${chalk.yellow(entity.name)} (${
            entity.properties.length
          } properties${entity.isJunction ? ', junction' : ''})`
        );
      });
    });
  } catch (error) {
    spinner.fail('Validation failed');
    console.error(chalk.red('\n❌ Error:'), error instanceof Error ? error.message : error);
    process.exit(1);
  }
}

// CLI definition
yargs(hideBin(process.argv))
  .scriptName('gen-bounded-context')
  .usage('$0 <command> [options]')
  .command(
    'generate <data-model>',
    'Generate BoundedContext code from data model',
    (yargs) => {
      return yargs
        .positional('data-model', {
          describe: 'Path to data model JSON file',
          type: 'string',
          demandOption: true,
        })
        .option('output-dir', {
          alias: 'o',
          describe: 'Output directory for generated code',
          type: 'string',
          default: '../../backend',
        })
        .option('namespace', {
          alias: 'n',
          describe: 'Override namespace from data model',
          type: 'string',
        })
        .option('base-namespace', {
          alias: 'b',
          describe: 'Base namespace prefix (default: Inventorization)',
          type: 'string',
        })
        .option('blueprint', {
          describe: 'Path to blueprint JSON file for architecture/layout strategy',
          type: 'string',
        })
        .option('skip-tests', {
          describe: 'Skip test project generation',
          type: 'boolean',
          default: false,
        })
        .option('dry-run', {
          describe: 'Preview what would be generated without writing files',
          type: 'boolean',
          default: false,
        })
        .option('force', {
          alias: 'f',
          describe: 'Force overwrite of existing .generated.cs files',
          type: 'boolean',
          default: true,
        })
        .option('concurrency', {
          alias: 'c',
          describe:
            'Max number of tasks (bounded contexts or generators within a phase) that run simultaneously. Omit for sequential execution.',
          type: 'number',
        });
    },
    (argv) => {
      generateCommand(argv['data-model'] as string, argv as unknown as GenerateOptions);
    }
  )
  .command(
    'validate <data-model>',
    'Validate data model JSON without generating code',
    (yargs) => {
      return yargs.positional('data-model', {
        describe: 'Path to data model JSON file',
        type: 'string',
        demandOption: true,
      }).option('blueprint', {
        describe: 'Path to blueprint JSON file for architecture/layout strategy',
        type: 'string',
      });
    },
    (argv) => {
      validateCommand(argv['data-model'] as string, argv.blueprint as string | undefined);
    }
  )
  .demandCommand(1, 'You must specify a command')
  .help()
  .alias('help', 'h')
  .version()
  .alias('version', 'v')
  .strict()
  .parse();
