export type BlueprintVersion = '1';

export type DtoStyleKind = 'class' | 'record';

export type PresentationKind = 'controllers' | 'minimal-api' | 'grpc';

export type DataAccessKind = 'ef-core' | 'ado-net';

export type DataProviderKind = 'npgsql';

export type UnitOfWorkKind = 'injected';

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
  },
};