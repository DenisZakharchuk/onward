import { ControllerGenerator } from '../../src/generators/ControllerGenerator';
import { BaseGenerator } from '../../src/generators/BaseGenerator';
import { BoundedContextGenerationContext, GenerationMetadata } from '../../src/models/DataModel';
import { Blueprint } from '../../src/models/Blueprint';

import { DataModel } from '../../src/models/DataModel';

const stubMetadata: GenerationMetadata = {
  generationStamp: 'test-stamp',
  generatedAt: new Date().toISOString(),
  sourceFile: 'test.json',
  baseNamespace: 'Onward',
};

const stubModel = {
  boundedContext: {
    name: 'Goods',
    namespace: 'Onward.Goods',
    description: 'Goods context',
    dataModel: {} as unknown as DataModel,
  },
  entities: [
    { name: 'Product', properties: [] },
    { name: 'Category', properties: [] },
  ],
} as unknown as BoundedContextGenerationContext;

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

describe('ControllerGenerator — authorizationEnabled in template context', () => {
  let generator: ControllerGenerator;
  const capturedContexts: Array<Record<string, unknown>> = [];

  beforeEach(() => {
    generator = new ControllerGenerator();
    generator.setMetadata(stubMetadata);
    capturedContexts.length = 0;

    jest
      .spyOn(BaseGenerator.prototype as unknown as { writeRenderedTemplate: (...args: unknown[]) => Promise<void> }, 'writeRenderedTemplate')
      .mockImplementation(async (_templates, context) => {
        capturedContexts.push(context as Record<string, unknown>);
      });
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it('authorizationEnabled=true for every entity when no blueprint (default)', async () => {
    await generator.generate(stubModel);
    expect(capturedContexts).toHaveLength(2);
    capturedContexts.forEach((ctx) => {
      expect(ctx.authorizationEnabled).toBe(true);
    });
  });

  it('authorizationEnabled=true for every entity with perDomain mode', async () => {
    generator.setBlueprint(blueprintWithMode('perDomain'));
    await generator.generate(stubModel);
    capturedContexts.forEach((ctx) => {
      expect(ctx.authorizationEnabled).toBe(true);
    });
  });

  it('authorizationEnabled=true for every entity with perContext mode', async () => {
    generator.setBlueprint(blueprintWithMode('perContext'));
    await generator.generate(stubModel);
    capturedContexts.forEach((ctx) => {
      expect(ctx.authorizationEnabled).toBe(true);
    });
  });

  it('authorizationEnabled=false for every entity with none mode', async () => {
    generator.setBlueprint(blueprintWithMode('none'));
    await generator.generate(stubModel);
    expect(capturedContexts).toHaveLength(2);
    capturedContexts.forEach((ctx) => {
      expect(ctx.authorizationEnabled).toBe(false);
    });
  });

  it('skips junction entities', async () => {
    const modelWithJunction = {
      boundedContext: stubModel.boundedContext,
      entities: [
        { name: 'Product', properties: [] },
        { name: 'ProductCategory', properties: [], isJunction: true },
      ],
    } as unknown as BoundedContextGenerationContext;
    await generator.generate(modelWithJunction);
    // Only 1 controller generated — junction entity skipped
    expect(capturedContexts).toHaveLength(1);
  });
});
