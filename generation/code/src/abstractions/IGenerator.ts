/**
 * Abstraction for code generators
 */

import { DataModel, GenerationMetadata } from '../models/DataModel';
import { IResultWriter } from './IResultWriter';

export interface IGenerator {
  /**
   * @deprecated Transitional lifecycle API. Prefer ADT-based generator contracts from GeneratorADT.
   */
  /**
   * Set generation metadata (called before generation)
   * @param metadata - Generation metadata
   */
  setMetadata(metadata: GenerationMetadata): void;

  /**
   * Set result writer (called before generation)
   * @param writer - Result writer implementation
   */
  setWriter(writer: IResultWriter): void;

  /**
   * Generate code for the data model
   * @param model - Data model to generate from
   */
  generate(model: DataModel): Promise<void>;
}
