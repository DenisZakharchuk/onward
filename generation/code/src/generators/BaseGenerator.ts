/**
 * Base generator class with common template rendering logic
 */

import * as Handlebars from 'handlebars';
import * as path from 'path';
import { FileManager } from '../utils/FileManager';
import { DataModel, GenerationMetadata } from '../models/DataModel';
import { IGenerator } from '../abstractions/IGenerator';
import { IResultWriter } from '../abstractions/IResultWriter';

export abstract class BaseGenerator implements IGenerator {
  protected templates: Map<string, HandlebarsTemplateDelegate> = new Map();
  protected templateDir: string;
  protected metadata?: GenerationMetadata;
  protected writer?: IResultWriter;

  constructor(templateDir?: string) {
    this.templateDir = templateDir || path.join(__dirname, '../../templates');
    this.registerHelpers();
  }

  /**
   * Set result writer (called by Orchestrator)
   */
  setWriter(writer: IResultWriter): void {
    this.writer = writer;
  }

  /**
   * Set generation metadata (called by Orchestrator)
   */
  setMetadata(metadata: GenerationMetadata): void {
    this.metadata = metadata;
  }

  /**
   * Register Handlebars helpers
   */
  private registerHelpers(): void {
    // Helper to check if last item in array
    Handlebars.registerHelper('isLast', function (index: number, array: unknown[]) {
      return index === array.length - 1;
    });

    // Helper for conditional rendering
    Handlebars.registerHelper('ifEquals', (arg1: unknown, arg2: unknown, options: Handlebars.HelperOptions) => {
      return arg1 === arg2 ? options.fn(this) : options.inverse(this);
    });

    // Helper for logical OR
    Handlebars.registerHelper('or', (...args: unknown[]) => {
      // Last argument is the options object
      const options = args[args.length - 1] as Handlebars.HelperOptions;
      const values = args.slice(0, -1);
      return values.some((v) => !!v) ? options.fn(this) : options.inverse(this);
    });

    // Helper for logical AND
    Handlebars.registerHelper('and', (...args: unknown[]) => {
      const options = args[args.length - 1] as Handlebars.HelperOptions;
      const values = args.slice(0, -1);
      return values.every((v) => !!v) ? options.fn(this) : options.inverse(this);
    });

    // Helper to uppercase first letter
    Handlebars.registerHelper('capitalize', function (str: string) {
      return str.charAt(0).toUpperCase() + str.slice(1);
    });

    // Helper to lowercase first letter
    Handlebars.registerHelper('camelCase', function (str: string) {
      return str.charAt(0).toLowerCase() + str.slice(1);
    });
  }

  /**
   * Load a template file
   */
  protected async loadTemplate(templateName: string): Promise<HandlebarsTemplateDelegate> {
    if (this.templates.has(templateName)) {
      return this.templates.get(templateName)!;
    }

    const templatePath = path.join(this.templateDir, templateName);
    const templateContent = await FileManager.readFile(templatePath);
    const template = Handlebars.compile(templateContent);

    this.templates.set(templateName, template);
    return template;
  }

  /**
   * Render a template with context (automatically injects generation metadata)
   */
  protected async renderTemplate(
    templateName: string,
    context: unknown
  ): Promise<string> {
    const template = await this.loadTemplate(templateName);
    
    // Inject generation metadata into context
    const enrichedContext = {
      ...context as Record<string, unknown>,
      generationStamp: this.metadata?.generationStamp,
      generatedAt: this.metadata?.generatedAt,
      sourceFile: this.metadata?.sourceFile,
      baseNamespace: this.metadata?.baseNamespace,
    };
    
    return template(enrichedContext);
  }

  /**
   * Write rendered template to file
   */
  protected async writeRenderedTemplate(
    templateName: string,
    context: unknown,
    outputPath: string,
    overwrite: boolean = false
  ): Promise<void> {
    if (!this.writer) {
      throw new Error('Result writer not set. Call setWriter() before generating.');
    }

    const content = await this.renderTemplate(templateName, context);
    await this.writer.write(outputPath, content, {
      overwrite,
      createDirectories: true,
    });
  }

  /**
   * Abstract method to be implemented by concrete generators
   */
  abstract generate(model: DataModel): Promise<void>;
}
