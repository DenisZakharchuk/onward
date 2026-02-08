#!/usr/bin/env node

/**
 * CLI entry point for the BoundedContext code generator
 */

import yargs from 'yargs';
import { hideBin } from 'yargs/helpers';
import chalk from 'chalk';
import ora from 'ora';
import { DataModelParser } from './parser/DataModelParser';
import { Orchestrator } from './orchestrator/Orchestrator';
import * as path from 'path';

interface GenerateOptions {
  outputDir: string;
  namespace?: string;
  skipTests: boolean;
  dryRun: boolean;
  force: boolean;
}

async function generateCommand(dataModelPath: string, options: GenerateOptions) {
  const spinner = ora('Loading data model...').start();

  try {
    // Parse and validate data model
    const parser = new DataModelParser();
    await parser.loadSchema();

    spinner.text = 'Validating data model...';
    const model = await parser.parseFromFile(dataModelPath);

    spinner.succeed(`Data model validated: ${chalk.green(model.boundedContext.name)}`);

    // Override namespace if provided
    if (options.namespace) {
      model.boundedContext.namespace = options.namespace;
    }

    // Resolve output directory
    const outputDir = path.resolve(options.outputDir);

    console.log(chalk.blue('\nGeneration Settings:'));
    console.log(`  BoundedContext: ${chalk.yellow(model.boundedContext.name)}`);
    console.log(`  Namespace: ${chalk.yellow(model.boundedContext.namespace)}`);
    console.log(`  Output Directory: ${chalk.yellow(outputDir)}`);
    console.log(`  Skip Tests: ${chalk.yellow(options.skipTests)}`);
    console.log(`  Dry Run: ${chalk.yellow(options.dryRun)}`);
    console.log(`  Force Overwrite: ${chalk.yellow(options.force)}\n`);

    if (options.dryRun) {
      console.log(chalk.yellow('🔍 DRY RUN - No files will be written\n'));
    }

    // Generate code
    spinner.start('Generating code...');
    const orchestrator = new Orchestrator({
      skipTests: options.skipTests,
      dryRun: options.dryRun,
      force: options.force,
    });

    await orchestrator.generate(model, outputDir);

    spinner.succeed(chalk.green('✅ Code generation completed successfully!'));

    if (!options.dryRun) {
      console.log(chalk.blue('\n📦 Next Steps:'));
      console.log(`  1. Review generated files in: ${chalk.yellow(outputDir)}`);
      console.log(`  2. Add custom business logic to ${chalk.yellow('.cs')} files`);
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

async function validateCommand(dataModelPath: string) {
  const spinner = ora('Validating data model...').start();

  try {
    const parser = new DataModelParser();
    await parser.loadSchema();

    const model = await parser.parseFromFile(dataModelPath);

    spinner.succeed(chalk.green('✅ Data model is valid!'));

    console.log(chalk.blue('\nData Model Summary:'));
    console.log(`  BoundedContext: ${chalk.yellow(model.boundedContext.name)}`);
    console.log(`  Namespace: ${chalk.yellow(model.boundedContext.namespace)}`);
    console.log(`  Entities: ${chalk.yellow(model.entities.length)}`);
    console.log(`  Relationships: ${chalk.yellow(model.relationships?.length || 0)}`);
    console.log(`  Enums: ${chalk.yellow(model.enums?.length || 0)}`);

    console.log(chalk.blue('\nEntities:'));
    model.entities.forEach((entity) => {
      console.log(
        `  - ${chalk.yellow(entity.name)} (${entity.properties.length} properties${entity.isJunction ? ', junction' : ''})`
      );
    });

    if (model.enums && model.enums.length > 0) {
      console.log(chalk.blue('\nEnums:'));
      model.enums.forEach((enumDef) => {
        console.log(`  - ${chalk.yellow(enumDef.name)} (${enumDef.values.length} values)`);
      });
    }
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
      });
    },
    (argv) => {
      validateCommand(argv['data-model'] as string);
    }
  )
  .demandCommand(1, 'You must specify a command')
  .help()
  .alias('help', 'h')
  .version()
  .alias('version', 'v')
  .strict()
  .parse();
