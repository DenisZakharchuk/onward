import { BoundedContextGenerationContext, GenerationMetadata } from '../models/DataModel';
import { IResultWriter } from '../abstractions/IResultWriter';
import { IGenerator } from '../abstractions/IGenerator';
import { LegacyGeneratorAdapter, LegacyGeneratorDescriptor } from './LegacyGeneratorAdapter';
import { GeneratorRegistry } from './GeneratorRegistry';
import {
  AbstractionGenerator,
  AdoNetApiProgramGenerator,
  AdoNetDataAccessGenerator,
  AdoNetDiGenerator,
  AdoNetMinimalApiProgramGenerator,
  ApiProgramGenerator,
  AppSettingsGenerator,
  ConfigurationGenerator,
  ControllerGenerator,
  DataAccessGenerator,
  DiGenerator,
  DtoGenerator,
  EntityGenerator,
  EnumGenerator,
  MetadataGenerator,
  MinimalApiEndpointsGenerator,
  MinimalApiProgramGenerator,
  ProjectGenerator,
  ProjectionDtoGenerator,
  ProjectionMapperGenerator,
  ProjectionMapperInterfaceGenerator,
  QueryBuilderGenerator,
  QueryControllerGenerator,
  SearchFieldsGenerator,
  SearchQueryValidatorGenerator,
  SearchServiceGenerator,
  ServiceGenerator,
  TestGenerator,
  ValidatorGenerator,
} from '../generators';

/**
 * Runtime callbacks that the registrar delegates to the Orchestrator for
 * context-aware resolution. Keeping these as an interface means GeneratorRegistrar
 * has no hard dependency on Orchestrator's internals.
 */
export interface GeneratorRegistrarContext {
  hasEnums(ctx: BoundedContextGenerationContext): boolean;
  resolveDataLayerKind(): 'ef-core' | 'ado-net';
}

/**
 * Owns all generator instantiation and registry wiring.
 * Extracted from Orchestrator so that Orchestrator remains focused on
 * coordination (scheduling, lifecycle) rather than construction.
 */
export class GeneratorRegistrar {
  /**
   * Instantiate every generator and register it with the provided registry.
   * @param registry  Fresh registry to populate.
   * @param ctx       Callbacks for runtime slot/condition resolution.
   */
  static registerAll(registry: GeneratorRegistry, ctx: GeneratorRegistrarContext): void {
    const reg = (
      generator: IGenerator,
      descriptor: LegacyGeneratorDescriptor & {
        dependsOn?: readonly string[];
        optionalSlot?: string;
      }
    ) => {
      const { dependsOn, optionalSlot, ...adapterDescriptor } = descriptor;
      registry.register({
        generator: new LegacyGeneratorAdapter(
          generator as {
            setMetadata(metadata: GenerationMetadata): void;
            setWriter(writer: IResultWriter): void;
            generate(model: BoundedContextGenerationContext): Promise<void>;
          },
          adapterDescriptor
        ),
        dependsOn,
        optionalSlot,
      });
    };

    reg(new MetadataGenerator(), {
      id: 'MetadataGenerator',
      domain: 'metadata',
      phase: 1,
      ambiguity: 'composite',
      provides: ['metadata'],
    });

    reg(new EnumGenerator(), {
      id: 'EnumGenerator',
      domain: 'metadata',
      phase: 2,
      ambiguity: 'optional',
      requires: ['metadata'],
      provides: ['enums'],
      applies: (model) => ctx.hasEnums(model),
    });

    reg(new EntityGenerator(), {
      id: 'EntityGenerator',
      domain: 'bl',
      phase: 3,
      ambiguity: 'variant',
      requires: ['metadata'],
      provides: ['entities'],
      dependsOn: ['MetadataGenerator'],
    });

    reg(new DtoGenerator(), {
      id: 'DtoGenerator',
      domain: 'dto',
      phase: 4,
      ambiguity: 'composite',
      requires: ['entities'],
      provides: ['dto'],
      dependsOn: ['EntityGenerator'],
    });

    reg(new ProjectionDtoGenerator(), {
      id: 'ProjectionDtoGenerator',
      domain: 'dto',
      phase: 4,
      ambiguity: 'variant',
      requires: ['entities'],
      provides: ['projection-dto'],
      dependsOn: ['EntityGenerator'],
    });

    reg(new ConfigurationGenerator(), {
      id: 'ConfigurationGenerator',
      domain: 'bl',
      phase: 5,
      ambiguity: 'variant',
      requires: ['entities'],
      provides: ['entity-configurations'],
      dependsOn: ['EntityGenerator'],
      optionalSlot: 'ef-core',
    });

    reg(new DataAccessGenerator(), {
      id: 'DataAccessGenerator',
      domain: 'bl',
      phase: 6,
      ambiguity: 'composite',
      requires: ['entity-configurations'],
      provides: ['data-access'],
      dependsOn: ['ConfigurationGenerator'],
      optionalSlot: 'ef-core',
    });

    reg(new AdoNetDataAccessGenerator(), {
      id: 'AdoNetDataAccessGenerator',
      domain: 'bl',
      phase: 6,
      ambiguity: 'composite',
      requires: ['entities'],
      provides: ['data-access'],
      dependsOn: ['EntityGenerator'],
      optionalSlot: 'ado-net',
    });

    reg(new AbstractionGenerator(), {
      id: 'AbstractionGenerator',
      domain: 'bl',
      phase: 7,
      ambiguity: 'composite',
      requires: ['entities'],
      provides: ['abstractions'],
      dependsOn: ['EntityGenerator'],
    });

    reg(new QueryBuilderGenerator(), {
      id: 'QueryBuilderGenerator',
      domain: 'query',
      phase: 7,
      ambiguity: 'deterministic',
      requires: ['entities'],
      provides: ['query-builder'],
      dependsOn: ['EntityGenerator'],
    });

    reg(new ProjectionMapperInterfaceGenerator(), {
      id: 'ProjectionMapperInterfaceGenerator',
      domain: 'query',
      phase: 7,
      ambiguity: 'deterministic',
      requires: ['projection-dto'],
      provides: ['projection-mapper-interface'],
      dependsOn: ['ProjectionDtoGenerator'],
    });

    reg(new ProjectionMapperGenerator(), {
      id: 'ProjectionMapperGenerator',
      domain: 'query',
      phase: 7,
      ambiguity: 'variant',
      requires: ['projection-mapper-interface'],
      provides: ['projection-mapper'],
      dependsOn: ['ProjectionMapperInterfaceGenerator'],
    });

    reg(new SearchFieldsGenerator(), {
      id: 'SearchFieldsGenerator',
      domain: 'query',
      phase: 7,
      ambiguity: 'variant',
      requires: ['entities'],
      provides: ['search-fields'],
      dependsOn: ['EntityGenerator'],
    });

    reg(new ValidatorGenerator(), {
      id: 'ValidatorGenerator',
      domain: 'bl',
      phase: 8,
      ambiguity: 'composite',
      requires: ['dto'],
      provides: ['validators'],
      dependsOn: ['DtoGenerator'],
    });

    reg(new SearchQueryValidatorGenerator(), {
      id: 'SearchQueryValidatorGenerator',
      domain: 'query',
      phase: 8,
      ambiguity: 'deterministic',
      requires: ['search-fields'],
      provides: ['search-query-validator'],
      dependsOn: ['SearchFieldsGenerator'],
    });

    reg(new ServiceGenerator(), {
      id: 'ServiceGenerator',
      domain: 'bl',
      phase: 9,
      ambiguity: 'deterministic',
      requires: ['validators'],
      provides: ['services'],
      dependsOn: ['ValidatorGenerator'],
    });

    reg(new SearchServiceGenerator(), {
      id: 'SearchServiceGenerator',
      domain: 'query',
      phase: 9,
      ambiguity: 'deterministic',
      requires: ['query-builder', 'projection-mapper'],
      provides: ['search-services'],
      dependsOn: ['QueryBuilderGenerator', 'ProjectionMapperGenerator'],
    });

    reg(new ControllerGenerator(), {
      id: 'ControllerGenerator',
      domain: 'api',
      phase: 10,
      ambiguity: 'deterministic',
      requires: ['services'],
      provides: ['controllers'],
      dependsOn: ['ServiceGenerator'],
      optionalSlot: 'controllers',
    });

    reg(new QueryControllerGenerator(), {
      id: 'QueryControllerGenerator',
      domain: 'api',
      phase: 10,
      ambiguity: 'variant',
      requires: ['search-services'],
      provides: ['query-controllers'],
      dependsOn: ['SearchServiceGenerator'],
      optionalSlot: 'controllers',
    });

    reg(new MinimalApiEndpointsGenerator(), {
      id: 'MinimalApiEndpointsGenerator',
      domain: 'api',
      phase: 10,
      ambiguity: 'deterministic',
      requires: ['services'],
      provides: ['minimal-api-endpoints'],
      dependsOn: ['ServiceGenerator'],
      optionalSlot: 'minimal-api',
    });

    reg(new DiGenerator(), {
      id: 'DiGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'composite',
      requires: ['services', 'search-services'],
      provides: ['di'],
      dependsOn: ['ServiceGenerator', 'SearchServiceGenerator'],
      optionalSlot: 'ef-core',
    });

    reg(new AdoNetDiGenerator(), {
      id: 'AdoNetDiGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'composite',
      requires: ['services', 'search-services', 'data-access'],
      provides: ['di'],
      dependsOn: ['ServiceGenerator', 'SearchServiceGenerator', 'AdoNetDataAccessGenerator'],
      optionalSlot: 'ado-net',
    });

    reg(new ApiProgramGenerator(), {
      id: 'ApiProgramGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'deterministic',
      requires: ['di'],
      provides: ['api-program'],
      dependsOn: ['DiGenerator'],
      optionalSlot: 'controllers',
      applies: () => ctx.resolveDataLayerKind() === 'ef-core',
    });

    reg(new AdoNetApiProgramGenerator(), {
      id: 'AdoNetApiProgramGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'deterministic',
      requires: ['di'],
      provides: ['api-program'],
      dependsOn: ['AdoNetDiGenerator'],
      optionalSlot: 'controllers',
      applies: () => ctx.resolveDataLayerKind() === 'ado-net',
    });

    reg(new MinimalApiProgramGenerator(), {
      id: 'MinimalApiProgramGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'deterministic',
      requires: ['di', 'minimal-api-endpoints'],
      provides: ['api-program'],
      dependsOn: ['DiGenerator', 'MinimalApiEndpointsGenerator'],
      optionalSlot: 'minimal-api',
      applies: () => ctx.resolveDataLayerKind() === 'ef-core',
    });

    reg(new AdoNetMinimalApiProgramGenerator(), {
      id: 'AdoNetMinimalApiProgramGenerator',
      domain: 'infra',
      phase: 11,
      ambiguity: 'deterministic',
      requires: ['di', 'minimal-api-endpoints'],
      provides: ['api-program'],
      dependsOn: ['AdoNetDiGenerator', 'MinimalApiEndpointsGenerator'],
      optionalSlot: 'minimal-api',
      applies: () => ctx.resolveDataLayerKind() === 'ado-net',
    });

    reg(new TestGenerator(), {
      id: 'TestGenerator',
      domain: 'tests',
      phase: 12,
      ambiguity: 'optional',
      requires: ['services', 'validators', 'projection-mapper'],
      provides: ['tests'],
      dependsOn: ['ServiceGenerator', 'ValidatorGenerator', 'ProjectionMapperGenerator'],
      optionalSlot: 'tests',
    });

    reg(new AppSettingsGenerator(), {
      id: 'AppSettingsGenerator',
      domain: 'api',
      phase: 13,
      ambiguity: 'deterministic',
      requires: ['api-program'],
      provides: ['app-settings'],
    });

    reg(new ProjectGenerator(), {
      id: 'ProjectGenerator',
      domain: 'project',
      phase: 13,
      ambiguity: 'variant',
      requires: ['di', 'api-program'],
      provides: ['projects'],
    });
  }
}
