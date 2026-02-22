import { IExecutionScheduler, SlotInfo } from '../abstractions/IExecutionScheduler';

/** The single slot used for all sequential execution. */
const SEQUENTIAL_SLOT: SlotInfo = { index: 1, total: 1 };

/**
 * Executes tasks one at a time in the order they were submitted.
 * Every task receives the same sequential SlotInfo ({ index: 1, total: 1 }).
 * This is the default scheduler — it preserves the existing sequential
 * behaviour and carries zero extra dependencies.
 */
export class SequentialScheduler implements IExecutionScheduler {
  readonly description = 'sequential';

  async run(tasks: ReadonlyArray<(slot: SlotInfo) => Promise<void>>): Promise<void> {
    for (const task of tasks) {
      await task(SEQUENTIAL_SLOT);
    }
  }
}
