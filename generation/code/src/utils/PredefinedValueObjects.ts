/**
 * Registry of predefined value objects — sealed classes in Onward.Base.Models that
 * implement IValueObject and are persisted via EF Core OwnsOne.
 *
 * To add a new value object:
 *   1. Create the class in backend/Onward.Base/Models/ implementing IValueObject.
 *   2. Add an entry here.
 *   3. Add the type name to CSharpType in DataModel.ts and data-model.schema.json.
 *   4. TypeMapper / ConfigurationGenerator / AbstractionGenerator detect it via
 *      isPredefinedValueObject() and apply the correct OwnsOne strategy automatically.
 */

export interface ValueObjectDescriptor {
  /** C# class name as it appears in generated code. */
  readonly csharpType: string;
  /** Fully qualified namespace (used for `using` directives). */
  readonly namespace: string;
  /** EF Core persistence strategy. Currently only 'OwnsOne' is supported. */
  readonly efStrategy: 'OwnsOne';
}

export const PREDEFINED_VALUE_OBJECTS: Record<string, ValueObjectDescriptor> = {
  DateTimeWithOffset: {
    csharpType: 'DateTimeWithOffset',
    namespace: 'Onward.Base.Models',
    efStrategy: 'OwnsOne',
  },
} as const;

/** Returns true when `type` is a registered predefined value object. */
export function isPredefinedValueObject(type: string): boolean {
  return Object.prototype.hasOwnProperty.call(PREDEFINED_VALUE_OBJECTS, type);
}

/** Returns the descriptor for a predefined value object, or undefined. */
export function getPredefinedValueObject(type: string): ValueObjectDescriptor | undefined {
  return PREDEFINED_VALUE_OBJECTS[type];
}
