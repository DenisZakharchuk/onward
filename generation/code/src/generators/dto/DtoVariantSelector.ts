import { BoundedContextGenerationContext } from '../../models/DataModel';
import { IDTOLibGenerator, IDTOVariantGenerator } from './DtoVariantContracts';

export class DtoVariantSelector implements IDTOLibGenerator {
  private variants: readonly IDTOVariantGenerator[] = [];

  init(generators: readonly IDTOVariantGenerator[]): void {
    this.variants = generators;
  }

  select(model: BoundedContextGenerationContext): IDTOVariantGenerator {
    if (this.variants.length === 0) {
      throw new Error('DTO variants are not initialized.');
    }

    const layout = model.boundedContext.dtoLayout ?? 'class';
    const selected = this.variants.find((variant) => variant.kind === layout);

    if (!selected) {
      const available = this.variants.map((variant) => variant.kind).join(', ');
      throw new Error(`DTO layout '${layout}' is not available. Supported variants: ${available}`);
    }

    return selected;
  }
}