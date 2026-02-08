/**
 * TypeScript models representing the data model schema
 */

export interface DataModel {
  boundedContext: BoundedContext;
  enums?: EnumDefinition[];
  entities: Entity[];
  relationships?: Relationship[];
}

export interface BoundedContext {
  name: string;
  namespace: string;
  description?: string;
  databaseName?: string;
  apiPort?: number;
}

export interface EnumDefinition {
  name: string;
  description?: string;
  values: EnumValue[];
}

export interface EnumValue {
  name: string;
  value?: number;
  description?: string;
}

export interface Entity {
  name: string;
  tableName?: string;
  schema?: string;
  description?: string;
  isJunction?: boolean;
  junctionMetadata?: JunctionMetadata;
  auditable?: boolean;
  properties: Property[];
  indexes?: Index[];
}

export interface JunctionMetadata {
  leftEntity: string;
  rightEntity: string;
  tier?: 1 | 3;
}

export interface Property {
  name: string;
  type: CSharpType | string; // string allows for custom enum types
  enumType?: string;
  required?: boolean;
  maxLength?: number;
  minLength?: number;
  precision?: number;
  scale?: number;
  defaultValue?: unknown;
  description?: string;
  displayName?: string;
  validationMessage?: string;
  isForeignKey?: boolean;
  referencedEntity?: string;
  navigationProperty?: string;
  isCollection?: boolean;
  collectionType?: string;
  includeInDto?: DtoInclusion;
  validation?: ValidationRules;
}

export interface DtoInclusion {
  create?: boolean;
  update?: boolean;
  details?: boolean;
  search?: boolean;
}

export interface ValidationRules {
  regex?: string;
  customValidator?: string;
  allowedValues?: unknown[];
  min?: number;
  max?: number;
}

export interface Index {
  columns: string[];
  isUnique?: boolean;
  name?: string;
}

export interface Relationship {
  type: RelationshipType;
  leftEntity: string;
  rightEntity: string;
  junctionEntity?: string;
  cardinality?: 'Required' | 'Optional';
  leftNavigationProperty?: string;
  rightNavigationProperty?: string;
  foreignKeyProperty?: string;
  onDelete?: 'Cascade' | 'Restrict' | 'SetNull' | 'NoAction';
  displayName?: string;
  description?: string;
}

export type RelationshipType = 'OneToOne' | 'OneToMany' | 'ManyToOne' | 'ManyToMany';

export type CSharpType =
  | 'string'
  | 'int'
  | 'long'
  | 'decimal'
  | 'double'
  | 'float'
  | 'bool'
  | 'DateTime'
  | 'DateTimeOffset'
  | 'Guid'
  | 'byte[]';

/**
 * Generation metadata injected into all templates
 */
export interface GenerationMetadata {
  generationStamp: string;      // Unique identifier for this generation run
  generatedAt: string;           // ISO timestamp
  sourceFile: string;            // Source data model file name
  baseNamespace: string;         // Base namespace prefix (e.g., 'Inventorization')
}

/**
 * Generated code context passed to templates
 */
export interface TemplateContext {
  boundedContext: BoundedContext;
  entity?: Entity;
  entities?: Entity[];
  relationships?: Relationship[];
  enums?: EnumDefinition[];
  property?: Property;
  properties?: Property[];
  relationship?: Relationship;
  // Generation metadata (injected automatically)
  generationStamp?: string;
  generatedAt?: string;
  sourceFile?: string;
  baseNamespace?: string;
  // Computed values
  namespace?: string;
  className?: string;
  tableName?: string;
  // Helper flags
  hasEnums?: boolean;
  hasJunctionEntities?: boolean;
  hasManyToManyRelationships?: boolean;
}
