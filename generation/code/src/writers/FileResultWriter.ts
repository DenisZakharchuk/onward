/**
 * File system-based result writer implementation
 */

import { IResultWriter, WriteOptions } from '../abstractions/IResultWriter';
import { FileManager } from '../utils/FileManager';
import * as path from 'path';

export class FileResultWriter implements IResultWriter {
  private baseOutputDir: string;

  constructor(baseOutputDir: string) {
    this.baseOutputDir = baseOutputDir;
  }

  async write(targetPath: string, content: string, options?: WriteOptions): Promise<void> {
    const fullPath = path.join(this.baseOutputDir, targetPath);
    await FileManager.writeFile(fullPath, content, {
      overwrite: options?.overwrite ?? true,
      createDirectories: options?.createDirectories ?? true,
    });
  }

  async ensureDirectory(directoryPath: string): Promise<void> {
    const fullPath = path.join(this.baseOutputDir, directoryPath);
    const fs = await import('fs-extra');
    await fs.ensureDir(fullPath);
  }

  async fileExists(targetPath: string): Promise<boolean> {
    const fullPath = path.join(this.baseOutputDir, targetPath);
    return await FileManager.fileExists(fullPath);
  }
}
