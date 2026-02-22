import { IExecutionScheduler } from '../abstractions/IExecutionScheduler';

/**
 * Executes tasks one at a time in the order they were submitted.
 * This is the default scheduler — it preserves the existing sequential
 * behaviour and carries zero extra dependencies.
 */
export class SequentialScheduler implements IExecutionScheduler {
  async run(tasks: ReadonlyArray<() => Promise<void>>): Promise<void> {
    for (const task of tasks) {
      await task();
    }
  }
}
