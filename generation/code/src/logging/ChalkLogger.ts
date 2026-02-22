import chalk from 'chalk';
import { ILogger } from '../abstractions/ILogger';
import { SlotInfo, isSequential } from '../abstractions/SlotInfo';

/**
 * Console logger that maps log levels to Chalk colours.
 *
 *   debug   → gray    (quiet diagnostics: stamps, paths, dry-run markers)
 *   info    → blue    (high-level progress headers)
 *   success → green   (completion confirmations)
 *   warn    → yellow  (recoverable issues)
 *   error   → red     (fatal failures)
 *   detail  → gray key + yellow value (structured key/value pairs)
 *   generator → cyan body, with a slot badge when running concurrently
 *
 * Chalk is intentionally confined to this class so the rest of the generator
 * tool has no dependency on a specific terminal-colour library.
 */
export class ChalkLogger implements ILogger {
  debug(message: string): void {
    console.log(chalk.gray(message));
  }

  info(message: string): void {
    console.log(chalk.blue(message));
  }

  success(message: string): void {
    console.log(chalk.green(message));
  }

  warn(message: string): void {
    console.log(chalk.yellow(message));
  }

  error(message: string): void {
    console.error(chalk.red(message));
  }

  detail(key: string, value: string | number): void {
    console.log(`  ${chalk.gray(key + ':')} ${chalk.yellow(String(value))}`);
  }

  /**
   * Emit a per-generator execution line.
   *
   * Sequential (total = 1):
   *   ⚙️  EntityGenerator — Running
   *
   * Concurrent (e.g. slot 2 of 4):
   *   ⚙️  [2/4] EntityGenerator — Running
   */
  generator(id: string, slot: SlotInfo, message: string): void {
    const slotBadge = isSequential(slot)
      ? ''
      : chalk.dim(`[${slot.index}/${slot.total}] `);
    console.log(chalk.cyan(`  ⚙️  ${slotBadge}${id}`) + chalk.gray(` — ${message}`));
  }
}
