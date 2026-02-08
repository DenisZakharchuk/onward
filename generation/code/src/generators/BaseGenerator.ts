/**
 * Base generator class with common template rendering logic
 */

import * as Handlebars from 'handlebars';
import * as path from 'path';
import { FileManager } from '../utils/FileManager';
import { DataModel } from '../models/DataModel';

export abstract class BaseGenerator {
  protected templates: Map<string, HandlebarsTemplateDelegate> = new Map();
  protected templateDir: string;

  constructor(templateDir?: string) {
    this.templateDir = templateDir || path.join(__dirname, '../../templates');
    this.registerHelpers();
  }

  /**
   * Register Handlebars helpers
   */
  private registerHelpers(): void {
    // Helper to check if last item in array
    Handlebars.registerHelper('isLast', function (index: number, array: any[]) {
      return index === array.length - 1;
    });

    // Helper for conditional rendering
    Handlebars.registerHelper('ifEquals', (arg1: any, arg2: any, options: any) => {
      return arg1 === arg2 ? options.fn(this) : options.inverse(this);
    });

    // Helper for logical OR
    Handlebars.registerHelper('or', (...args: any[]) => {
      // Last argument is the options object
      const options = args[args.length - 1];
      const values = args.slice(0, -1);
      return values.some((v) => !!v) ? options.fn(this) : options.inverse(this);
    });

    // Helper for logical AND
    Handlebars.registerHelper('and', (...args: any[]) => {
      const options = args[args.length - 1];
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
   * Render a template with context
   */
  protected async renderTemplate(
    templateName: string,
    context: any
  ): Promise<string> {
    const template = await this.loadTemplate(templateName);
    return template(context);
  }

  /**
   * Write rendered template to file
   */
  protected async writeRenderedTemplate(
    templateName: string,
    context: any,
    outputPath: string,
    overwrite: boolean = false
  ): Promise<void> {
    const content = await this.renderTemplate(templateName, context);
    await FileManager.writeFile(outputPath, content, {
      overwrite,
      createDirectories: true,
    });
  }

  /**
   * Abstract method to be implemented by concrete generators
   */
  abstract generate(model: DataModel, outputDir: string): Promise<void>;
}
