/**
 * Public abstractions (interfaces) for dependency injection
 */

export { IGenerator } from './IGenerator';
export { IModelProvider } from './IModelProvider';
export { IResultWriter, WriteOptions } from './IResultWriter';
export {
	IArrayGenerator,
	IArraySlot,
	IBoundedContextGenerator,
	IComplexGenerator,
	ICompositeGenerator,
	ICompositeSlot,
	IControlledGenerator,
	IDeterministicGenerator,
	IGeneratorCore,
	IGeneratorExecutionContext,
	IOptionalGenerator,
	IProvidesCapabilities,
	IRequiresCapabilities,
	ISimpleGenerator,
	IVariantGenerator,
	IVariantSelection,
	IVariantSlot,
	GeneratorAmbiguity,
	GeneratorBL,
	GeneratorPhase,
} from './GeneratorADT';
