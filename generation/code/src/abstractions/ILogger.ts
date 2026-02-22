import { SlotInfo } from './SlotInfo';

/**
 * Logger abstraction for the generation tool.
 *
 * Each method corresponds to a semantic log level; implementations decide
 * how to render them (e.g. chalk colors, JSON, silent).
 *
 * Level guidance:
 *  - debug   — internal diagnostics (stamps, paths, dry-run markers)
 *  - info    — high-level progress updates (starting, bounded-context headers)
 *  - success — completion confirmations (✅ lines)
 *  - warn    — recoverable issues
 *  - error   — fatal / unexpected failures
 *  - detail  — structured key/value pairs inside a progress section
 *  - generator — per-generator execution line, annotated with slot info
 */
export interface ILogger {
  debug(message: string): void;
  info(message: string): void;
  success(message: string): void;
  warn(message: string): void;
  error(message: string): void;

  /**
   * Emit a structured key = value diagnostic line (e.g. "  Stamp: abc123").
   * Implementations typically render the key in a muted color and the value
   * highlighted (e.g. yellow).
   */
  detail(key: string, value: string | number): void;

  /**
   * Log a single generator execution step, tagged with the concurrency slot it
   * ran in.  When total > 1 the slot label makes parallel execution visible.
   *
   * @param id      Generator identifier (e.g. "EntityGenerator")
   * @param slot    Slot context emitted by the scheduler
   * @param message Short status message (e.g. "Running", "Skipped (dry run)")
   */
  generator(id: string, slot: SlotInfo, message: string): void;
}
