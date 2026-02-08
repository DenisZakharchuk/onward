/**
 * Abstraction for loading and validating data models
 */

import { DataModel } from '../models/DataModel';

export interface IModelProvider {
  /**
   * Load and validate a data model
   * @param source - Source identifier (e.g., file path, URL, etc.)
   * @returns Validated data model
   */
  load(source: string): Promise<DataModel>;

  /**
   * Validate a data model without loading
   * @param source - Source identifier
   * @returns Validation result with errors if any
   */
  validate(source: string): Promise<{
    valid: boolean;
    errors: string[];
    model?: DataModel;
  }>;
}
