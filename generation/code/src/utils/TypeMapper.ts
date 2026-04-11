/**
 * Maps JSON schema types to C# types
 */

import { Property } from '../models/DataModel';

export class TypeMapper {
  /**
   * Convert JSON type to C# type
   */
  static toCSharpType(type: string, isNullable: boolean = false): string {
    if (!type) return 'object';
    let csharpType: string;

    switch (type.toLowerCase()) {
      case 'string':
        csharpType = 'string';
        break;
      case 'int':
      case 'integer':
        csharpType = 'int';
        break;
      case 'long':
        csharpType = 'long';
        break;
      case 'decimal':
        csharpType = 'decimal';
        break;
      case 'double':
        csharpType = 'double';
        break;
      case 'float':
        csharpType = 'float';
        break;
      case 'bool':
      case 'boolean':
        csharpType = 'bool';
        break;
      case 'datetime':
        csharpType = 'DateTime';
        break;
      case 'datetimeoffset':
        csharpType = 'DateTimeOffset';
        break;
      case 'guid':
      case 'uuid':
        csharpType = 'Guid';
        break;
      case 'byte[]':
      case 'bytes':
        csharpType = 'byte[]';
        break;
      default:
        // Assumed to be a custom type (enum, entity, etc.)
        csharpType = type;
    }

    // Add nullable marker for value types (strings and byte[] are already nullable)
    if (
      isNullable &&
      csharpType !== 'string' &&
      csharpType !== 'byte[]' &&
      !this.isReferenceType(type)
    ) {
      return `${csharpType}?`;
    }

    return csharpType;
  }

  /**
   * Get default value for a C# type
   */
  static getDefaultValue(type: string): string {
    if (!type) return 'default!';
    switch (type.toLowerCase()) {
      case 'string':
        return 'string.Empty';
      case 'int':
      case 'long':
      case 'float':
      case 'double':
      case 'decimal':
        return '0';
      case 'bool':
      case 'boolean':
        return 'false';
      case 'guid':
      case 'uuid':
        return 'Guid.Empty';
      case 'datetime':
        return 'DateTime.MinValue';
      case 'datetimeoffset':
        return 'DateTimeOffset.MinValue';
      case 'byte[]':
        return 'Array.Empty<byte>()';
      default:
        return 'default!';
    }
  }

  /**
   * Check if a type is a reference type (for nullability)
   */
  static isReferenceType(type: string): boolean {
    if (!type) return true;
    const valueTypes = [
      'int',
      'long',
      'decimal',
      'double',
      'float',
      'bool',
      'datetime',
      'datetimeoffset',
      'guid',
    ];
    return !valueTypes.includes(type.toLowerCase());
  }

  /**
   * Check if a type is numeric
   */
  static isNumericType(type: string): boolean {
    if (!type) return false;
    const numericTypes = ['int', 'long', 'decimal', 'double', 'float'];
    return numericTypes.includes(type.toLowerCase());
  }

  /**
   * Check if a type is a string
   */
  static isStringType(type: string): boolean {
    if (!type) return false;
    return type.toLowerCase() === 'string';
  }

  /**
   * Get the appropriate SQL column type for EF Core configuration
   */
  static toSqlType(property: Property): string | null {
    if (property.type === 'decimal' && property.precision) {
      return `decimal(${property.precision}, ${property.scale || 0})`;
    }
    if (property.type === 'string' && property.maxLength) {
      return `nvarchar(${property.maxLength})`;
    }
    if (property.type === 'DateTimeOffset') {
      return 'timestamptz';
    }
    return null; // Use default mapping
  }

  /**
   * Get validation attribute for property
   */
  static getValidationAttributes(property: Property): string[] {
    const attributes: string[] = [];

    if (property.required && this.isStringType(property.type)) {
      attributes.push('[Required]');
    }

    if (property.maxLength) {
      if (property.minLength) {
        attributes.push(`[StringLength(${property.maxLength}, MinimumLength = ${property.minLength})]`);
      } else {
        attributes.push(`[StringLength(${property.maxLength})]`);
      }
    }

    // Range and RegularExpression validations are intentionally omitted from DTO annotations.
    // Business-rule validations (range, format, email, etc.) are enforced exclusively by the
    // generated IValidator<T> classes in BL/Validators/, which are called by DataServiceBase
    // before every create/update operation.

    return attributes;
  }

}
