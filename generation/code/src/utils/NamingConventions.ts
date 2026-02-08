/**
 * Naming convention utilities for consistent code generation
 */

export class NamingConventions {
  /**
   * Convert entity name to Create DTO name
   * Example: User -> CreateUserDTO
   */
  static toCreateDtoName(entityName: string): string {
    return `Create${entityName}DTO`;
  }

  /**
   * Convert entity name to Update DTO name
   * Example: User -> UpdateUserDTO
   */
  static toUpdateDtoName(entityName: string): string {
    return `Update${entityName}DTO`;
  }

  /**
   * Convert entity name to Delete DTO name
   * Example: User -> DeleteUserDTO
   */
  static toDeleteDtoName(entityName: string): string {
    return `Delete${entityName}DTO`;
  }

  /**
   * Convert entity name to Details DTO name
   * Example: User -> UserDetailsDTO
   */
  static toDetailsDtoName(entityName: string): string {
    return `${entityName}DetailsDTO`;
  }

  /**
   * Convert entity name to Search DTO name
   * Example: User -> UserSearchDTO
   */
  static toSearchDtoName(entityName: string): string {
    return `${entityName}SearchDTO`;
  }

  /**
   * Convert entity name to Creator class name
   * Example: User -> UserCreator
   */
  static toCreatorName(entityName: string): string {
    return `${entityName}Creator`;
  }

  /**
   * Convert entity name to Modifier class name
   * Example: User -> UserModifier
   */
  static toModifierName(entityName: string): string {
    return `${entityName}Modifier`;
  }

  /**
   * Convert entity name to Mapper class name
   * Example: User -> UserMapper
   */
  static toMapperName(entityName: string): string {
    return `${entityName}Mapper`;
  }

  /**
   * Convert entity name to SearchProvider class name
   * Example: User -> UserSearchProvider
   */
  static toSearchProviderName(entityName: string): string {
    return `${entityName}SearchProvider`;
  }

  /**
   * Convert entity name to validator name
   * Example: User, 'Create' -> CreateUserValidator
   */
  static toValidatorName(entityName: string, dtoType: 'Create' | 'Update'): string {
    return `${dtoType}${entityName}Validator`;
  }

  /**
   * Convert entity name to DataService interface name
   * Example: User -> IUserDataService
   */
  static toDataServiceInterfaceName(entityName: string): string {
    return `I${entityName}DataService`;
  }

  /**
   * Convert entity name to DataService class name
   * Example: User -> UserDataService
   */
  static toDataServiceClassName(entityName: string): string {
    return `${entityName}DataService`;
  }

  /**
   * Convert entity name to EntityConfiguration class name
   * Example: User -> UserConfiguration
   */
  static toEntityConfigurationName(entityName: string): string {
    return `${entityName}Configuration`;
  }

  /**
   * Convert entity name to controller name
   * Example: User -> UsersController
   */
  static toControllerName(entityName: string): string {
    return `${this.pluralize(entityName)}Controller`;
  }

  /**
   * Convert bounded context name to DbContext name
   * Example: Auth -> AuthDbContext
   */
  static toDbContextName(contextName: string): string {
    return `${contextName}DbContext`;
  }

  /**
   * Convert bounded context name to UnitOfWork interface name
   * Example: Auth -> IAuthUnitOfWork
   */
  static toUnitOfWorkInterfaceName(contextName: string): string {
    return `I${contextName}UnitOfWork`;
  }

  /**
   * Convert bounded context name to UnitOfWork class name
   * Example: Auth -> AuthUnitOfWork
   */
  static toUnitOfWorkClassName(contextName: string): string {
    return `${contextName}UnitOfWork`;
  }

  /**
   * Convert two entity names to RelationshipManager name
   * Example: User, Role -> UserRoleRelationshipManager
   */
  static toRelationshipManagerName(leftEntity: string, rightEntity: string): string {
    return `${leftEntity}${rightEntity}RelationshipManager`;
  }

  /**
   * Convert junction entity to EntityId PropertyAccessor name
   * Example: UserRole -> UserRoleEntityIdPropertyAccessor
   */
  static toEntityIdAccessorName(junctionEntity: string): string {
    return `${junctionEntity}EntityIdPropertyAccessor`;
  }

  /**
   * Convert junction entity to RelatedEntityId PropertyAccessor name
   * Example: UserRole -> UserRoleRelatedEntityIdPropertyAccessor
   */
  static toRelatedEntityIdAccessorName(junctionEntity: string): string {
    return `${junctionEntity}RelatedEntityIdPropertyAccessor`;
  }

  /**
   * Convert entity name to table name (pluralized by default)
   * Example: User -> Users
   */
  static toTableName(entityName: string, customTableName?: string): string {
    return customTableName || this.pluralize(entityName);
  }

  /**
   * Convert entity name to DbSet property name (pluralized)
   * Example: User -> Users
   */
  static toDbSetName(entityName: string): string {
    return this.pluralize(entityName);
  }

  /**
   * Simple pluralization (basic English rules)
   */
  static pluralize(word: string): string {
    if (word.endsWith('s') || word.endsWith('x') || word.endsWith('ch') || word.endsWith('sh')) {
      return `${word}es`;
    }
    if (word.endsWith('y') && !/[aeiou]y$/i.test(word)) {
      return `${word.slice(0, -1)}ies`;
    }
    return `${word}s`;
  }

  /**
   * Convert to camelCase
   * Example: UserRole -> userRole
   */
  static toCamelCase(str: string): string {
    return str.charAt(0).toLowerCase() + str.slice(1);
  }

  /**
   * Convert to kebab-case
   * Example: UserRole -> user-role
   */
  static toKebabCase(str: string): string {
    return str.replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase();
  }

  /**
   * Convert to snake_case
   * Example: UserRole -> user_role
   */
  static toSnakeCase(str: string): string {
    return str.replace(/([a-z])([A-Z])/g, '$1_$2').toLowerCase();
  }

  /**
   * Generate file path for entity-related file
   * Example: User, 'Creators', '.generated.cs' -> Creators/UserCreator.generated.cs
   */
  static toFilePath(
    _entityName: string,
    folder: string,
    className: string,
    isGenerated: boolean = true
  ): string {
    const suffix = isGenerated ? '.generated.cs' : '.cs';
    return `${folder}/${className}${suffix}`;
  }
}
