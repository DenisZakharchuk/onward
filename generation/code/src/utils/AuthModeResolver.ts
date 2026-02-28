import { Blueprint, AuthorizationMode, DEFAULT_BLUEPRINT } from '../models/Blueprint';
import { BoundedContext, OnlineAuthConfig } from '../models/DataModel';

/** Resolved online-auth settings with all defaults applied. */
export interface ResolvedOnlineAuthConfig extends Required<OnlineAuthConfig> {}

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

  /**
   * Returns `true` when the blueprint is configured for online token introspection.
   * Requires mode='perDomain' and authMode='online'.
   */
  static isOnlineAuth(blueprint: Blueprint | undefined): boolean {
    const auth = blueprint?.boundedContext.authorization;
    return auth?.mode === 'perDomain' && (auth as { authMode?: string }).authMode === 'online';
  }

  /**
   * Returns the resolved online-auth configuration, merging data-model values with defaults.
   * authServiceUrl is taken from boundedContext.authModel.onlineAuth.authServiceUrl.
   * All other fields fall back to sensible defaults.
   */
  static resolveOnlineAuthConfig(
    _blueprint: Blueprint | undefined,
    boundedContext: BoundedContext
  ): ResolvedOnlineAuthConfig {
    const raw = boundedContext.authModel?.onlineAuth;
    return {
      authServiceUrl: raw?.authServiceUrl ?? '',
      cacheTtlSeconds: raw?.cacheTtlSeconds ?? 30,
      failOpen: raw?.failOpen ?? false,
      transport: raw?.transport ?? 'Http',
      timeoutSeconds: raw?.timeoutSeconds ?? 5,
    };
  }
}
