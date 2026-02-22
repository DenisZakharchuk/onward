/**
 * TypeScript models representing the data model schema
 */

/**
 * Top-level domain model — the root input to the generator CLI.
 * Contains shared enums and one or more bounded contexts, each with their own DataModel.
 */
export interface DomainModel {
  /** Domain-wide shared enum definitions (available to every bounded context). */
  enums?: EnumDefinition[];
  /** One or more bounded contexts to generate. */
  boundedContexts: BoundedContext[];
}

/**
 * Entity and relationship definitions scoped to a single bounded context.
 * Previously this was the top-level type; it is now nested inside BoundedContext.
 */
export interface DataModel {
  entities: Entity[];
  relationships?: Relationship[];
}

/**
 * Flattened per-context view passed to every generator.
 * Carries the same field names that generators already use (boundedContext, entities, enums,
 * relationships), derived by merging domain-level enums with context-level enums and
 * unwrapping BoundedContext.dataModel.
 */
export interface BoundedContextGenerationContext {
  boundedContext: BoundedContext;
  /** Merged enums: domain-level + context-level (context-level wins on name collision). */
  enums: EnumDefinition[];
  /** Shorthand for boundedContext.dataModel.entities */
  entities: Entity[];
  /** Shorthand for boundedContext.dataModel.relationships */
  relationships: Relationship[];
}

export interface JwtConfig {
  /** JWT issuer value (e.g., 'Inventorization.Auth') */
  issuer: string;
  /** JWT audience value (e.g., 'Inventorization.Client') */
  audience: string;
  /** Access token expiration in minutes */
  expirationMinutes: number;
  /** Development environment token expiration in minutes (defaults to expirationMinutes) */
  devExpirationMinutes?: number;
}

export interface BoundedContext {
  name: string;
  namespace: string;
  description?: string;
  databaseName?: string;
  apiPort?: number;
  /** PostgreSQL port (defaults to 5432) */
  dbPort?: number;
  /** When set, appsettings generation includes a JwtSettings section */
  jwt?: JwtConfig;
  dtoLayout?: 'class' | 'record';
  /**
   * Ownership configuration for the bounded context.
   * When set, entities marked with `owned: true` will extend OwnedBaseEntity<TOwnership>
   * and the DI registration will call AddOwnershipServices<TOwnership, TFactory>().
   */
  ownership?: OwnershipConfig;
  /**
   * Enum definitions local to this bounded context.
   * At generation time they are merged with the domain-level DomainModel.enums.
   */
  enums?: EnumDefinition[];
  /** Entity and relationship definitions for this bounded context. */
  dataModel: DataModel;
}

export interface OwnershipConfig {
  /** Whether any entity in this context uses ownership (drives DI registration). */
  enabled: boolean;
  /**
   * The concrete OwnershipValueObject type scaffolded for this context.
   * Defaults to 'UserTenantOwnership' when omitted.
   */
  valueObject?: string;
  /**
   * The IOwnershipFactory implementation to register.
   * Defaults to '<valueObject>Factory' when omitted.
   */
  factory?: string;
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
  /**
   * When true, the entity extends OwnedBaseEntity<TOwnership> instead of BaseEntity,
   * and the query builder / search service are generated with ownership awareness.
   * Requires boundedContext.ownership.enabled = true.
   */
  owned?: boolean;
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
 * Generated code context passed to templates.
 * Derived from BoundedContextGenerationContext — field names are intentionally identical.
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
