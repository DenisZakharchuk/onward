import { IGenerator } from '../abstractions/IGenerator';
import {
  GeneratorAmbiguity,
  GeneratorBL,
  GeneratorPhase,
  IControlledGenerator,
  IGeneratorExecutionContext,
} from '../abstractions/GeneratorADT';
import { DataModel } from '../models/DataModel';

export interface LegacyGeneratorDescriptor {
  id: string;
  domain: GeneratorBL;
  phase: GeneratorPhase;
  outputs?: readonly string[];
  ambiguity?: GeneratorAmbiguity;
  requires?: readonly string[];
  provides?: readonly string[];
  applies?: (model: DataModel) => boolean;
}

export class LegacyGeneratorAdapter implements IControlledGenerator<DataModel, IGeneratorExecutionContext> {
  readonly id: string;
  readonly domain: GeneratorBL;
  readonly phase: GeneratorPhase;
  readonly outputs: readonly string[];
  readonly ambiguity: GeneratorAmbiguity;
  readonly requires: readonly string[];
  readonly provides: readonly string[];

  private readonly descriptor: LegacyGeneratorDescriptor;
  private readonly legacyGenerator: IGenerator;

  constructor(legacyGenerator: IGenerator, descriptor: LegacyGeneratorDescriptor) {
    this.legacyGenerator = legacyGenerator;
    this.descriptor = descriptor;
    this.id = descriptor.id;
    this.domain = descriptor.domain;
    this.phase = descriptor.phase;
    this.outputs = descriptor.outputs ?? [];
    this.ambiguity = descriptor.ambiguity ?? 'deterministic';
    this.requires = descriptor.requires ?? [];
    this.provides = descriptor.provides ?? [];
  }

  applies(model: DataModel): boolean {
    return this.descriptor.applies ? this.descriptor.applies(model) : true;
  }

  async generate(model: DataModel, context: IGeneratorExecutionContext): Promise<void> {
    this.legacyGenerator.setMetadata(context.metadata);
    this.legacyGenerator.setWriter(context.writer);
    await this.legacyGenerator.generate(model);
  }
}