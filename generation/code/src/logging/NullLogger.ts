import { ILogger } from '../abstractions/ILogger';
import { SlotInfo } from '../abstractions/SlotInfo';

/**
 * No-op logger that silently discards all messages.
 *
 * Useful in unit tests where console output is undesirable, or when constructing
 * an {@link Orchestrator} without a logging dependency.
 */
export class NullLogger implements ILogger {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  debug(_message: string): void { /* noop */ }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  info(_message: string): void { /* noop */ }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  success(_message: string): void { /* noop */ }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  warn(_message: string): void { /* noop */ }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  error(_message: string): void { /* noop */ }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  detail(_key: string, _value: string | number): void { /* noop */ }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  generator(_id: string, _slot: SlotInfo, _message: string): void { /* noop */ }
}
