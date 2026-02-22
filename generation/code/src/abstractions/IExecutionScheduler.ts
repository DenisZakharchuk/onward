import { SlotInfo } from './SlotInfo';

/**
 * Abstraction for executing a batch of async tasks with controlled concurrency.
 *
 * Callers submit an array of slot-aware async callbacks.
 * Each callback receives a {@link SlotInfo} describing which worker slot it was
 * assigned to, enabling callers to annotate log lines without coupling to any
 * specific scheduler implementation.
 *
 * Application logic (generators, orchestrator) never imports concurrency
 * primitives directly — they depend only on this interface.
 */
export interface IExecutionScheduler {
  /**
   * Human-readable label describing the active execution strategy.
   * Used by loggers to display which scheduler is in effect.
   * @example 'sequential' | 'concurrent (n=4)'
   */
  readonly description: string;

  /**
   * Run all tasks in the batch according to the scheduler's concurrency policy.
   * Each task receives a {@link SlotInfo} identifying the slot it ran in.
   * Resolves when every task in the batch has completed.
   * Rejects with the first task error (remaining tasks are still awaited before
   * the rejection propagates, matching Promise.all semantics).
   */
  run(tasks: ReadonlyArray<(slot: SlotInfo) => Promise<void>>): Promise<void>;
}

export { SlotInfo };
