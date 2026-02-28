export type BlueprintVersion = '1';

export type DtoStyleKind = 'class' | 'record';

export type PresentationKind = 'controllers' | 'minimal-api' | 'grpc';

export type DataAccessKind = 'ef-core' | 'ado-net';

export type DataProviderKind = 'npgsql';

export type UnitOfWorkKind = 'injected';

// ---------------------------------------------------------------------------
// Authorization types
// ---------------------------------------------------------------------------

/** Whether JWT auth is verified by the Auth service, generated per-context, or absent. */
export type AuthorizationMode = 'perDomain' | 'perContext' | 'none';

/** Authorization config when mode is 'perDomain' — delegates to Onward.Auth. */
export interface BlueprintAuthorizationPerDomain {
  mode: 'perDomain';
  /**
   * Auth strategy for this bounded context.
   * - 'local'  (default) — validates JWT signature/lifetime locally only; emits AddOnwardJwtAuth.
   * - 'online' — additionally introspects each token against the Auth Service at runtime;
   *              emits AddOnwardOnlineAuth. Requires authModel.onlineAuth in the data model.
   */
  authMode?: 'local' | 'online';
}

/** Authorization config when mode is 'perContext' — auth entities generated inside this bounded context. */
export interface BlueprintAuthorizationPerContext {
  mode: 'perContext';
}

/** Authorization config when mode is 'none' — no auth infrastructure generated. */
export interface BlueprintAuthorizationNone {
  mode: 'none';
}

export type BlueprintAuthorization =
  | BlueprintAuthorizationPerDomain
  | BlueprintAuthorizationPerContext
  | BlueprintAuthorizationNone;

// ---------------------------------------------------------------------------

export interface Blueprint {
  version: BlueprintVersion;
  boundedContext: BlueprintBoundedContext;
}

export interface BlueprintBoundedContext {
  common?: {
    enums?: 'SmartEnums';
  };
  presentation: BlueprintPresentation;
  dataService: BlueprintDataService;
  /**
   * Authorization strategy for the bounded context.
   * Defaults to `{ mode: 'perDomain' }` when absent.
   */
  authorization?: BlueprintAuthorization;
}

export type BlueprintPresentation =
  | {
      kind: 'controllers';
    }
  | {
      kind: 'minimal-api';
    }
  | {
      kind: 'grpc';
    };

export interface BlueprintDataService {
  dto: DtoStyleKind;
  uow: UnitOfWorkKind;
  dataAccess: BlueprintDataAccess;
  domain: 'default';
}

export type BlueprintDataAccess =
  | {
      orm: {
        kind: 'ef-core';
        provider: DataProviderKind;
      };
      entities?: 'immutable';
    }
  | {
      ado: {
        provider: DataProviderKind;
        dialect?: 'pgsql';
      };
      entities?: 'immutable';
    };

export const DEFAULT_BLUEPRINT: Blueprint = {
  version: '1',
  boundedContext: {
    common: {
      enums: 'SmartEnums',
    },
    presentation: {
      kind: 'controllers',
    },
    dataService: {
      dto: 'class',
      uow: 'injected',
      dataAccess: {
        orm: {
          kind: 'ef-core',
          provider: 'npgsql',
        },
        entities: 'immutable',
      },
      domain: 'default',
    },
    authorization: {
      mode: 'perDomain',
    },
  },
};