import { IVariantSlot } from '../../abstractions/GeneratorADT';
import { IGenerator } from '../../abstractions/IGenerator';

export type DtoLayout = 'class' | 'record';

export interface IDTOLayoutGenerator extends IGenerator {
  readonly kind: DtoLayout;
}

export interface IClassDTOGenerator extends IDTOLayoutGenerator {
  readonly kind: 'class';
}

export interface IRecordDTOGenerator extends IDTOLayoutGenerator {
  readonly kind: 'record';
}

export type IDTOVariantGenerator = IClassDTOGenerator | IRecordDTOGenerator;

export interface IDTOLibGenerator extends IVariantSlot<IDTOVariantGenerator> {}