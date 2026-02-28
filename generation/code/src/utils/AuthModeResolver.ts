import { Blueprint, AuthorizationMode, DEFAULT_BLUEPRINT } from '../models/Blueprint';

/**
 * Resolves authorization-related flags from a Blueprint instance.
 * Uses DEFAULT_BLUEPRINT defaults when the blueprint (or authorization section) is absent.
 */
export class AuthModeResolver {
  /**
   * Returns the effective authorization mode for a blueprint.
   * Falls back to the DEFAULT_BLUEPRINT mode ('perDomain') when unset.
   */
  static resolveMode(blueprint: Blueprint | undefined): AuthorizationMode {
    return (
      blueprint?.boundedContext.authorization?.mode ??
      DEFAULT_BLUEPRINT.boundedContext.authorization?.mode ??
      'perDomain'
    );
  }

  /**
   * Returns `true` when the auth mode requires JWT bearer authentication
   * to be wired into the generated Program.cs and controllers.
   */
  static isAuthorizationEnabled(blueprint: Blueprint | undefined): boolean {
    return this.resolveMode(blueprint) !== 'none';
  }
}
