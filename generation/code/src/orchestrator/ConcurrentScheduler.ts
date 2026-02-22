import pLimit from 'p-limit';
import { IExecutionScheduler, SlotInfo } from '../abstractions/IExecutionScheduler';

/**
 * Executes tasks with a bounded concurrency limit using p-limit.
 *
 * Each task callback receives a {@link SlotInfo} identifying the 1-based slot
 * index it was assigned (1..concurrency).  Slot indices are taken from a pool
 * before each task runs and returned after it completes, so callers can use the
 * slot number to label log lines and make parallel execution visible.
 *
 * All knowledge of the semaphore / rate-limiting mechanism is isolated here.
 * The rest of the system only sees IExecutionScheduler.
 *
 * @example
 *   const scheduler = new ConcurrentScheduler(4); // max 4 tasks at a time
 */
export class ConcurrentScheduler implements IExecutionScheduler {
  private readonly limiter: ReturnType<typeof pLimit>;
  private readonly concurrency: number;

  constructor(concurrency: number) {
    if (concurrency < 1) {
      throw new RangeError(`ConcurrentScheduler: concurrency must be >= 1, got ${concurrency}`);
    }
    this.concurrency = concurrency;
    this.limiter = pLimit(concurrency);
  }

  async run(tasks: ReadonlyArray<(slot: SlotInfo) => Promise<void>>): Promise<void> {
    // Pool of available slot indices (1-based).  p-limit ensures at most
    // `concurrency` tasks run simultaneously, so the pool always has a slot
    // ready when a task enters its critical section.
    const availableSlots: number[] = Array.from({ length: this.concurrency }, (_, i) => i + 1);
    const total = this.concurrency;

    await Promise.all(
      tasks.map((task) =>
        this.limiter(async () => {
          const index = availableSlots.shift()!;
          try {
            await task({ index, total });
          } finally {
            availableSlots.push(index);
          }
        })
      )
    );
  }
}
