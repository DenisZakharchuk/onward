/**
 * Carries concurrency-slot context from an IExecutionScheduler into task callbacks.
 *
 * Both sequential and concurrent schedulers emit a SlotInfo so consumers can
 * annotate log lines without knowing which scheduler is in use.
 *
 * @example
 *   // Sequential:   { index: 1, total: 1 }
 *   // Concurrent 4: { index: 2, total: 4 }  – whichever worker picked up the task
 */
export interface SlotInfo {
  /** 1-based index of the worker slot that executed this task. Always 1 for sequential. */
  index: number;
  /** Total number of concurrent slots. Always 1 for sequential. */
  total: number;
}

/** Helper: returns true when the scheduler is effectively sequential (one slot). */
export function isSequential(slot: SlotInfo): boolean {
  return slot.total === 1;
}
