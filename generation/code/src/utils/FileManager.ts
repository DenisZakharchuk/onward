/**
 * File system operations manager
 */

import * as fs from 'fs-extra';
import * as path from 'path';

export interface FileWriteOptions {
  overwrite?: boolean;
  createDirectories?: boolean;
  backup?: boolean;
}

export class FileManager {
  /**
   * Write content to file with optional backup
   */
  static async writeFile(
    filePath: string,
    content: string,
    options: FileWriteOptions = {}
  ): Promise<void> {
    const {
      overwrite = false,
      createDirectories = true,
      backup = false,
    } = options;

    const absolutePath = path.resolve(filePath);
    const directory = path.dirname(absolutePath);

    // Create directories if needed
    if (createDirectories) {
      await fs.ensureDir(directory);
    }

    // Check if file exists
    const exists = await fs.pathExists(absolutePath);

    if (exists && !overwrite) {
      // For .generated.cs files, always overwrite
      if (absolutePath.endsWith('.generated.cs')) {
        // Create backup if requested
        if (backup) {
          await fs.copy(absolutePath, `${absolutePath}.backup`);
        }
      } else {
        // Don't overwrite custom files
        console.log(`Skipping existing file: ${filePath}`);
        return;
      }
    }

    // Write the file
    await fs.writeFile(absolutePath, content, 'utf-8');
  }

  /**
   * Read file content
   */
  static async readFile(filePath: string): Promise<string> {
    return fs.readFile(path.resolve(filePath), 'utf-8');
  }

  /**
   * Check if file exists
   */
  static async fileExists(filePath: string): Promise<boolean> {
    return fs.pathExists(path.resolve(filePath));
  }

  /**
   * Create directory
   */
  static async ensureDir(dirPath: string): Promise<void> {
    await fs.ensureDir(path.resolve(dirPath));
  }

  /**
   * Delete file
   */
  static async deleteFile(filePath: string): Promise<void> {
    await fs.remove(path.resolve(filePath));
  }

  /**
   * Check if file is a generated file
   */
  static isGeneratedFile(filePath: string): boolean {
    return filePath.endsWith('.generated.cs');
  }

  /**
   * Get the custom file path for a generated file
   * Example: UserCreator.generated.cs -> UserCreator.cs
   */
  static getCustomFilePath(generatedFilePath: string): string {
    return generatedFilePath.replace('.generated.cs', '.cs');
  }

  /**
   * Get relative path from base directory
   */
  static getRelativePath(from: string, to: string): string {
    return path.relative(from, to);
  }

  /**
   * Join path segments
   */
  static joinPath(...segments: string[]): string {
    return path.join(...segments);
  }

  /**
   * Read JSON file
   */
  static async readJson<T>(filePath: string): Promise<T> {
    return fs.readJson(path.resolve(filePath));
  }

  /**
   * Write JSON file
   */
  static async writeJson(filePath: string, data: unknown, options?: fs.WriteOptions): Promise<void> {
    await fs.writeJson(path.resolve(filePath), data, { spaces: 2, ...options });
  }

  /**
   * Copy file
   */
  static async copyFile(source: string, destination: string): Promise<void> {
    await fs.copy(path.resolve(source), path.resolve(destination));
  }

  /**
   * List files in directory
   */
  static async listFiles(dirPath: string, pattern?: RegExp): Promise<string[]> {
    const absolutePath = path.resolve(dirPath);
    const exists = await fs.pathExists(absolutePath);

    if (!exists) {
      return [];
    }

    const files = await fs.readdir(absolutePath);

    if (pattern) {
      return files.filter((file) => pattern.test(file));
    }

    return files;
  }

  /**
   * Create stub file if it doesn't exist
   */
  static async createStubIfNotExists(
    filePath: string,
    content: string
  ): Promise<boolean> {
    const exists = await this.fileExists(filePath);

    if (!exists) {
      await this.writeFile(filePath, content, { createDirectories: true });
      return true;
    }

    return false;
  }
}
