/**
 * Abstraction for loading and validating data models
 */

import { DomainModel } from '../models/DataModel';

export interface IModelProvider {
  /**
   * Load and validate a domain model
   * @param source - Source identifier (e.g., file path, URL, etc.)
   * @returns Validated domain model
   */
  load(source: string): Promise<DomainModel>;

  /**
   * Validate a domain model without loading
   * @param source - Source identifier
   * @returns Validation result with errors if any
   */
  validate(source: string): Promise<{
    valid: boolean;
    errors: string[];
    model?: DomainModel;
  }>;
}
