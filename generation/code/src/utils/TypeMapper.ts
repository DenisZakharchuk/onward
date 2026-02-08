/**
 * Maps JSON schema types to C# types
 */

import { Property } from '../models/DataModel';

export class TypeMapper {
  /**
   * Convert JSON type to C# type
   */
  static toCSharpType(type: string, isNullable: boolean = false): string {
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
    const numericTypes = ['int', 'long', 'decimal', 'double', 'float'];
    return numericTypes.includes(type.toLowerCase());
  }

  /**
   * Check if a type is a string
   */
  static isStringType(type: string): boolean {
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

    if (property.validation?.regex) {
      attributes.push(`[RegularExpression(@"${property.validation.regex}")]`);
    }

    if (property.validation?.min !== undefined || property.validation?.max !== undefined) {
      if (property.validation.min !== undefined && property.validation.max !== undefined) {
        attributes.push(`[Range(${property.validation.min}, ${property.validation.max})]`);
      } else if (property.validation.min !== undefined) {
        attributes.push(`[Range(${property.validation.min}, ${this.getMaxValueForType(property.type)})]`);
      } else if (property.validation.max !== undefined) {
        attributes.push(`[Range(${this.getMinValueForType(property.type)}, ${property.validation.max})]`);
      }
    }

    return attributes;
  }

  /**
   * Get maximum value for numeric type
   */
  private static getMaxValueForType(type: string): string {
    switch (type.toLowerCase()) {
      case 'int':
        return 'int.MaxValue';
      case 'long':
        return 'long.MaxValue';
      case 'decimal':
        return 'decimal.MaxValue';
      case 'double':
        return 'double.MaxValue';
      case 'float':
        return 'float.MaxValue';
      default:
        return 'int.MaxValue';
    }
  }

  /**
   * Get minimum value for numeric type
   */
  private static getMinValueForType(type: string): string {
    switch (type.toLowerCase()) {
      case 'int':
        return 'int.MinValue';
      case 'long':
        return 'long.MinValue';
      case 'decimal':
        return 'decimal.MinValue';
      case 'double':
        return 'double.MinValue';
      case 'float':
        return 'float.MinValue';
      default:
        return 'int.MinValue';
    }
  }
}
