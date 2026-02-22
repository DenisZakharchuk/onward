import { IControlledGenerator, IGeneratorExecutionContext } from '../abstractions/GeneratorADT';
import { BoundedContextGenerationContext } from '../models/DataModel';

export interface GeneratorRegistration {
  generator: IControlledGenerator<BoundedContextGenerationContext, IGeneratorExecutionContext>;
  dependsOn?: readonly string[];
  optionalSlot?: string;
}

export class GeneratorRegistry {
  private readonly registrations = new Map<string, GeneratorRegistration>();

  register(registration: GeneratorRegistration): void {
    const id = registration.generator.id;

    if (this.registrations.has(id)) {
      throw new Error(`Duplicate generator registration: ${id}`);
    }

    this.registrations.set(id, registration);
  }

  list(): GeneratorRegistration[] {
    return Array.from(this.registrations.values());
  }

  resolveExecutionPlan(
    ctx: BoundedContextGenerationContext,
    _context: IGeneratorExecutionContext,
    enabledSlots: ReadonlySet<string>
  ): GeneratorRegistration[] {
    const candidates = this.list().filter((registration) => {
      if (registration.optionalSlot && !enabledSlots.has(registration.optionalSlot)) {
        return false;
      }

      return registration.generator.applies(ctx);
    });

    this.validateDependencies(candidates);
    return this.sortByPhaseAndDependencies(candidates);
  }

  private validateDependencies(registrations: readonly GeneratorRegistration[]): void {
    const ids = new Set(registrations.map((registration) => registration.generator.id));

    for (const registration of registrations) {
      const dependencies = registration.dependsOn ?? [];

      for (const dependency of dependencies) {
        if (!ids.has(dependency)) {
          throw new Error(
            `Generator '${registration.generator.id}' depends on missing generator '${dependency}'.`
          );
        }
      }
    }
  }

  private sortByPhaseAndDependencies(
    registrations: readonly GeneratorRegistration[]
  ): GeneratorRegistration[] {
    const byId = new Map(registrations.map((registration) => [registration.generator.id, registration]));
    const sortedByPhase = [...registrations].sort((left, right) => {
      if (left.generator.phase === right.generator.phase) {
        return left.generator.id.localeCompare(right.generator.id);
      }

      return left.generator.phase - right.generator.phase;
    });

    const visited = new Set<string>();
    const visiting = new Set<string>();
    const executionOrder: GeneratorRegistration[] = [];

    const visit = (generatorId: string): void => {
      if (visited.has(generatorId)) {
        return;
      }

      if (visiting.has(generatorId)) {
        throw new Error(`Circular generator dependency detected at '${generatorId}'.`);
      }

      const registration = byId.get(generatorId);

      if (!registration) {
        throw new Error(`Generator not found while building execution plan: '${generatorId}'.`);
      }

      visiting.add(generatorId);

      for (const dependency of registration.dependsOn ?? []) {
        visit(dependency);
      }

      visiting.delete(generatorId);
      visited.add(generatorId);
      executionOrder.push(registration);
    };

    for (const registration of sortedByPhase) {
      visit(registration.generator.id);
    }

    return executionOrder;
  }
}