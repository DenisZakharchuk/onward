/**
 * Maps C# / data-model types to TypeScript types for client generation.
 */
export class TypeScriptTypeMapper {
  /**
   * Convert a data model type string to a TypeScript type string.
   * @param type      Raw type from the data model JSON (e.g. 'Guid', 'int', 'string?')
   * @param nullable  Whether to append ` | null`
   */
  static toTsType(type: string, nullable: boolean = false): string {
    const base = this.baseType(type.replace('?', '').trim());
    return nullable || type.endsWith('?') ? `${base} | null` : base;
  }

  private static baseType(type: string): string {
    switch (type.toLowerCase()) {
      case 'guid':
      case 'uuid':
      case 'string':
        return 'string';
      case 'int':
      case 'integer':
      case 'long':
      case 'decimal':
      case 'double':
      case 'float':
        return 'number';
      case 'bool':
      case 'boolean':
        return 'boolean';
      case 'datetime':
      case 'datetimeoffset':
        return 'string'; // ISO-8601 string over the wire
      case 'byte[]':
      case 'bytes':
        return 'string'; // base64
      default:
        // Enum or complex type — keep as-is (TS enum names are preserved)
        return type;
    }
  }

  /**
   * TypeScript default value for a given data-model type.
   * Used as the default in optional DTO fields.
   */
  static defaultValue(type: string): string {
    const t = type.replace('?', '').trim().toLowerCase();
    switch (t) {
      case 'guid':
      case 'uuid':
      case 'string':
        return "''";
      case 'int':
      case 'integer':
      case 'long':
      case 'decimal':
      case 'double':
      case 'float':
        return '0';
      case 'bool':
      case 'boolean':
        return 'false';
      case 'datetime':
      case 'datetimeoffset':
        return "''";
      default:
        return 'undefined';
    }
  }
}
