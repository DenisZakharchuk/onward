/**
 * DTO generator - ADT variant orchestrator (class/record)
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel } from '../models/DataModel';
import { ClassDtoVariantGenerator } from './dto/ClassDtoVariantGenerator';
import { DtoVariantSelector } from './dto/DtoVariantSelector';
import { IDTOVariantGenerator } from './dto/DtoVariantContracts';
import { RecordDtoVariantGenerator } from './dto/RecordDtoVariantGenerator';

export class DtoGenerator extends BaseGenerator {
  private readonly selector: DtoVariantSelector;
  private readonly variants: readonly IDTOVariantGenerator[];

  constructor() {
    super();
    this.selector = new DtoVariantSelector();
    this.variants = [
      new ClassDtoVariantGenerator(),
      new RecordDtoVariantGenerator(),
    ];
    this.selector.init(this.variants);
  }

  async generate(model: DataModel): Promise<void> {
    if (!this.metadata) {
      throw new Error('Generation metadata is not set for DtoGenerator.');
    }

    if (!this.writer) {
      throw new Error('Result writer is not set for DtoGenerator.');
    }

    const selected = this.selector.select(model);
    selected.setMetadata(this.metadata);
    selected.setWriter(this.writer);
    await selected.generate(model);
  }
}
