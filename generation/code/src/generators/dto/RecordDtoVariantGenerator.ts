import { IRecordDTOGenerator } from './DtoVariantContracts';
import { AbstractDtoVariantGenerator } from './AbstractDtoVariantGenerator';

export class RecordDtoVariantGenerator extends AbstractDtoVariantGenerator implements IRecordDTOGenerator {
  readonly kind = 'record' as const;

  protected getCreateTemplateName(): string[] {
    return ['dto/record/create-dto.hbs', 'create-dto.record.cs.hbs'];
  }

  protected getUpdateTemplateName(): string[] {
    return ['dto/record/update-dto.hbs', 'update-dto.record.cs.hbs'];
  }

  protected getDeleteTemplateName(): string[] {
    return ['dto/record/delete-dto.hbs', 'delete-dto.record.cs.hbs'];
  }

  protected getInitTemplateName(): string[] {
    return ['dto/record/init-dto.hbs', 'init-dto.record.cs.hbs'];
  }

  protected getDetailsTemplateName(): string[] {
    return ['dto/record/details-dto.hbs', 'details-dto.record.cs.hbs'];
  }

  protected getSearchTemplateName(): string[] {
    return ['dto/record/search-dto.hbs', 'search-dto.record.cs.hbs'];
  }
}
