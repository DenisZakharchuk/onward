/**
 * Abstraction for writing generation results
 */

export interface WriteOptions {
  overwrite?: boolean;
  createDirectories?: boolean;
}

export interface IResultWriter {
  /**
   * Write generated content to target location
   * @param targetPath - Target path (relative to output directory)
   * @param content - Generated content
   * @param options - Write options
   */
  write(targetPath: string, content: string, options?: WriteOptions): Promise<void>;

  /**
   * Ensure directory structure exists
   * @param directoryPath - Directory path to ensure
   */
  ensureDirectory(directoryPath: string): Promise<void>;

  /**
   * Check if file exists
   * @param targetPath - File path to check
   */
  fileExists(targetPath: string): Promise<boolean>;
}
