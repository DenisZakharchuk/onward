import { AuthModeResolver } from '../../src/utils/AuthModeResolver';
import { Blueprint } from '../../src/models/Blueprint';

function blueprintWithMode(
  mode: 'perDomain' | 'perContext' | 'none',
  authServiceUrl?: string
): Blueprint {
  return {
    version: '1',
    boundedContext: {
      presentation: { kind: 'controllers' },
      dataService: {
        dto: 'class',
        uow: 'injected',
        dataAccess: { orm: { kind: 'ef-core', provider: 'npgsql' } },
        domain: 'default',
      },
      authorization:
        mode === 'perDomain'
          ? { mode: 'perDomain', authServiceUrl }
          : mode === 'perContext'
          ? { mode: 'perContext' }
          : { mode: 'none' },
    },
  };
}

describe('AuthModeResolver.resolveMode', () => {
  it('returns perDomain when blueprint is undefined (default)', () => {
    expect(AuthModeResolver.resolveMode(undefined)).toBe('perDomain');
  });

  it('returns perDomain when authorization is absent from blueprint', () => {
    const bp: Blueprint = {
      version: '1',
      boundedContext: {
        presentation: { kind: 'controllers' },
        dataService: {
          dto: 'class',
          uow: 'injected',
          dataAccess: { orm: { kind: 'ef-core', provider: 'npgsql' } },
          domain: 'default',
        },
      },
    };
    expect(AuthModeResolver.resolveMode(bp)).toBe('perDomain');
  });

  it('returns perDomain for perDomain blueprint', () => {
    expect(AuthModeResolver.resolveMode(blueprintWithMode('perDomain'))).toBe('perDomain');
  });

  it('returns perContext for perContext blueprint', () => {
    expect(AuthModeResolver.resolveMode(blueprintWithMode('perContext'))).toBe('perContext');
  });

  it('returns none for none blueprint', () => {
    expect(AuthModeResolver.resolveMode(blueprintWithMode('none'))).toBe('none');
  });
});

describe('AuthModeResolver.isAuthorizationEnabled', () => {
  it('is true when blueprint is undefined (defaults to perDomain)', () => {
    expect(AuthModeResolver.isAuthorizationEnabled(undefined)).toBe(true);
  });

  it('is true for perDomain mode', () => {
    expect(AuthModeResolver.isAuthorizationEnabled(blueprintWithMode('perDomain'))).toBe(true);
  });

  it('is true for perContext mode', () => {
    expect(AuthModeResolver.isAuthorizationEnabled(blueprintWithMode('perContext'))).toBe(true);
  });

  it('is false for none mode', () => {
    expect(AuthModeResolver.isAuthorizationEnabled(blueprintWithMode('none'))).toBe(false);
  });

  it('is true for perDomain with optional authServiceUrl', () => {
    expect(
      AuthModeResolver.isAuthorizationEnabled(
        blueprintWithMode('perDomain', 'http://auth-svc:5012')
      )
    ).toBe(true);
  });
});
