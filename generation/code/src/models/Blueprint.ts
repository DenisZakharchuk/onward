export type BlueprintVersion = '1';

export type DtoStyleKind = 'class' | 'record';

export type PresentationKind = 'controllers' | 'minimal-api' | 'grpc';

export type DataAccessKind = 'ef-core' | 'ado-net';

export type DataProviderKind = 'npgsql';

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
  uow: BlueprintUnitOfWork;
  domain: 'default';
}

export type BlueprintUnitOfWork =
  | {
      dataLayer: {
        kind: 'ef-core';
        provider: DataProviderKind;
      };
      entities?: 'immutable';
    }
  | {
      dataLayer: {
        kind: 'ado-net';
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
      uow: {
        dataLayer: {
          kind: 'ef-core',
          provider: 'npgsql',
        },
        entities: 'immutable',
      },
      domain: 'default',
    },
  },
};