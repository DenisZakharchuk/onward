/**
 * Generation stamp utilities
 */

import * as crypto from 'crypto';

export class GenerationStamp {
  /**
   * Generate a unique generation stamp (16 characters)
   * Format: YYYYMMDD-XXXXXXXX (datestamp + 8-char hash)
   */
  static create(): string {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const datestamp = `${year}${month}${day}`;

    // Generate 8-character hash from timestamp + random data
    const hash = crypto
      .createHash('sha256')
      .update(`${now.getTime()}-${Math.random()}`)
      .digest('hex')
      .substring(0, 8);

    return `${datestamp}-${hash}`;
  }

  /**
   * Get formatted timestamp for generation metadata
   */
  static getTimestamp(): string {
    return new Date().toISOString().replace('T', ' ').substring(0, 19) + ' UTC';
  }
}
