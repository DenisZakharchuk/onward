import pLimit from 'p-limit';
import { IExecutionScheduler } from '../abstractions/IExecutionScheduler';

/**
 * Executes tasks with a bounded concurrency limit using p-limit.
 *
 * All knowledge of the semaphore / rate-limiting mechanism is isolated here.
 * The rest of the system only sees IExecutionScheduler.
 *
 * @example
 *   const scheduler = new ConcurrentScheduler(4); // max 4 tasks at a time
 */
export class ConcurrentScheduler implements IExecutionScheduler {
  private readonly limiter: ReturnType<typeof pLimit>;

  constructor(concurrency: number) {
    if (concurrency < 1) {
      throw new RangeError(`ConcurrentScheduler: concurrency must be >= 1, got ${concurrency}`);
    }
    this.limiter = pLimit(concurrency);
  }

  async run(tasks: ReadonlyArray<() => Promise<void>>): Promise<void> {
    await Promise.all(tasks.map((task) => this.limiter(task)));
  }
}
