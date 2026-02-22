/**
 * Abstraction for executing a batch of async tasks with controlled concurrency.
 *
 * Callers submit a flat array of zero-argument async functions.
 * The scheduler decides how many of them run simultaneously.
 * Application logic (generators, orchestrator) never imports concurrency
 * primitives directly — they depend only on this interface.
 */
export interface IExecutionScheduler {
  /**
   * Run all tasks in the batch according to the scheduler's concurrency policy.
   * Resolves when every task in the batch has completed.
   * Rejects with the first task error (remaining tasks are still awaited before
   * the rejection propagates, matching Promise.all semantics).
   */
  run(tasks: ReadonlyArray<() => Promise<void>>): Promise<void>;
}
