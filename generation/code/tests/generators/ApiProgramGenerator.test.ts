import { ApiProgramGenerator } from '../../src/generators/ApiProgramGenerator';
import { BaseGenerator } from '../../src/generators/BaseGenerator';
import { BoundedContextGenerationContext, GenerationMetadata } from '../../src/models/DataModel';
import { Blueprint } from '../../src/models/Blueprint';

import { DataModel } from '../../src/models/DataModel';

// Minimal metadata stub
const stubMetadata: GenerationMetadata = {
  generationStamp: 'test-stamp',
  generatedAt: new Date().toISOString(),
  sourceFile: 'test.json',
  baseNamespace: 'Onward',
};

// Minimal generation context stub
const stubModel = {
  boundedContext: {
    name: 'Goods',
    namespace: 'Onward.Goods',
    description: 'Goods context',
    dataModel: {} as unknown as DataModel,
  },
  entities: [],
} as unknown as BoundedContextGenerationContext;

// Helpers to build Blueprint stubs
function blueprintWithMode(mode: 'perDomain' | 'perContext' | 'none'): Blueprint {
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
          ? { mode: 'perDomain' }
          : mode === 'perContext'
          ? { mode: 'perContext' }
          : { mode: 'none' },
    },
  };
}

describe('ApiProgramGenerator — authorizationEnabled in template context', () => {
  let generator: ApiProgramGenerator;
  let capturedContext: Record<string, unknown>;

  beforeEach(() => {
    generator = new ApiProgramGenerator();
    generator.setMetadata(stubMetadata);

    // Spy on writeRenderedTemplate (protected) — capture the context arg
    jest
      .spyOn(BaseGenerator.prototype as unknown as { writeRenderedTemplate: (...args: unknown[]) => Promise<void> }, 'writeRenderedTemplate')
      .mockImplementation(async (_templates, context) => {
        capturedContext = context as Record<string, unknown>;
      });
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it('authorizationEnabled=true when no blueprint (default perDomain)', async () => {
    await generator.generate(stubModel);
    expect(capturedContext.authorizationEnabled).toBe(true);
  });

  it('authorizationEnabled=true when mode is perDomain', async () => {
    generator.setBlueprint(blueprintWithMode('perDomain'));
    await generator.generate(stubModel);
    expect(capturedContext.authorizationEnabled).toBe(true);
  });

  it('authorizationEnabled=true when mode is perContext', async () => {
    generator.setBlueprint(blueprintWithMode('perContext'));
    await generator.generate(stubModel);
    expect(capturedContext.authorizationEnabled).toBe(true);
  });

  it('authorizationEnabled=false when mode is none', async () => {
    generator.setBlueprint(blueprintWithMode('none'));
    await generator.generate(stubModel);
    expect(capturedContext.authorizationEnabled).toBe(false);
  });
});
