import { DataModel, GenerationMetadata } from '../models/DataModel';
import { Blueprint } from '../models/Blueprint';
import { IGenerator } from './IGenerator';
import { IResultWriter } from './IResultWriter';

export type GeneratorAmbiguity = 'deterministic' | 'variant' | 'composite' | 'optional';

export type GeneratorDomain =
  | 'project'
  | 'dto'
  | 'domain'
  | 'api'
  | 'query'
  | 'metadata'
  | 'tests'
  | 'infra';

export type GeneratorPhase = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13;

export interface IGeneratorExecutionContext {
  metadata: GenerationMetadata;
  writer: IResultWriter;
  blueprint?: Blueprint;
  options: {
    dryRun: boolean;
    force: boolean;
    skipTests: boolean;
  };
}

export interface IGeneratorCore<M, Ctx, Out extends string = string> {
  id: string;
  domain: GeneratorDomain;
  phase: GeneratorPhase;
  outputs: readonly Out[];
  ambiguity: GeneratorAmbiguity;
  applies(model: M): boolean;
  generate(model: M, context: Ctx): Promise<void>;
}

export interface IDeterministicGenerator<M, Ctx, Out extends string = string>
  extends IGeneratorCore<M, Ctx, Out> {
  ambiguity: 'deterministic';
}

export interface IVariantSelection<Kind extends string> {
  kind: Kind;
}

export interface IVariantGenerator<
  M,
  Ctx,
  Variant extends IVariantSelection<string>,
  Out extends string = string,
> extends IGeneratorCore<M, Ctx, Out> {
  ambiguity: 'variant';
  variants: readonly Variant['kind'][];
  selectVariant(model: M, context: Ctx): Variant;
}

export interface ICompositeGenerator<M, Ctx, Part extends string, Out extends string = string>
  extends IGeneratorCore<M, Ctx, Out> {
  ambiguity: 'composite';
  parts: readonly Part[];
  runPart(part: Part, model: M, context: Ctx): Promise<void>;
}

export interface IOptionalGenerator<M, Ctx, Slot extends string, Out extends string = string>
  extends IGeneratorCore<M, Ctx, Out> {
  ambiguity: 'optional';
  slot: Slot;
  enabled(model: M, context: Ctx): boolean;
}

export interface IRequiresCapabilities<Capability extends string> {
  requires: readonly Capability[];
}

export interface IProvidesCapabilities<Capability extends string> {
  provides: readonly Capability[];
}

export type IControlledGenerator<M, Ctx, Out extends string = string> =
  IGeneratorCore<M, Ctx, Out> &
  IRequiresCapabilities<string> &
  IProvidesCapabilities<string>;

export interface ISimpleGenerator extends IGenerator {}

export interface IArrayGenerator<TGenerator extends IGenerator> extends IGenerator {
  init(generators: TGenerator[]): void;
}

export interface IComplexGenerator<TGenerators extends Record<string, IGenerator>> extends IGenerator {
  init(generators: TGenerators): void;
}

export interface IVariantSlot<TVariant> {
  init(generators: readonly TVariant[]): void;
  select(model: DataModel): TVariant;
}

export interface ICompositeSlot<TGenerators extends Record<string, unknown>> {
  init(generators: TGenerators): void;
}

export interface IArraySlot<TVariant> {
  init(generators: readonly TVariant[]): void;
}

export interface IBoundedContextGenerator extends IComplexGenerator<Record<string, IGenerator>> {}