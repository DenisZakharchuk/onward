import { IClassDTOGenerator } from './DtoVariantContracts';
import { AbstractDtoVariantGenerator } from './AbstractDtoVariantGenerator';

export class ClassDtoVariantGenerator extends AbstractDtoVariantGenerator implements IClassDTOGenerator {
  readonly kind = 'class' as const;

  protected getCreateTemplateName(): string[] {
    return ['dto/class/create-dto.hbs', 'create-dto.cs.hbs'];
  }

  protected getUpdateTemplateName(): string[] {
    return ['dto/class/update-dto.hbs', 'update-dto.cs.hbs'];
  }

  protected getDeleteTemplateName(): string[] {
    return ['dto/class/delete-dto.hbs', 'delete-dto.cs.hbs'];
  }

  protected getInitTemplateName(): string[] {
    return ['dto/class/init-dto.hbs', 'init-dto.cs.hbs'];
  }

  protected getDetailsTemplateName(): string[] {
    return ['dto/class/details-dto.hbs', 'details-dto.cs.hbs'];
  }

  protected getSearchTemplateName(): string[] {
    return ['dto/class/search-dto.hbs', 'search-dto.cs.hbs'];
  }
}
