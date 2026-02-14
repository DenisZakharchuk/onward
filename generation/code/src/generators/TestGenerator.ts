/**
 * Test generator - creates unit tests and instantiation tests
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property } from '../models/DataModel';
import { NamingConventions } from '../utils/NamingConventions';
import { TypeMapper } from '../utils/TypeMapper';
import * as path from 'path';

interface Assignment {
  name: string;
  value: string;
}

interface InvalidCase {
  propertyName: string;
  invalidValue: string;
}

export class TestGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const testsProjectPath = `${baseNamespace}.${contextName}.API.Tests`;
    const servicesDir = path.join(testsProjectPath, 'Services');
    const validatorsDir = path.join(testsProjectPath, 'Validators');
    const mappersDir = path.join(testsProjectPath, 'Mappers');
    const instantiationDir = path.join(testsProjectPath, 'Instantiation');

    for (const entity of model.entities) {
      if (entity.isJunction) {
        continue;
      }

      const createAssignments = this.buildDtoAssignments(entity, model, 'create');
      const updateAssignments = this.buildDtoAssignments(entity, model, 'update');
      const constructorArgs = this.buildConstructorArgs(entity, model);
      const mapAssertions = this.buildMapAssertions(entity);
      const invalidCreateCases = this.buildInvalidCases(entity, 'create');
      const invalidUpdateCases = this.buildInvalidCases(entity, 'update');

      const context = {
        baseNamespace,
        namespace,
        contextName,
        entityName: entity.name,
        projectionName: `${entity.name}Projection`,
        searchServiceName: `${entity.name}SearchService`,
        dataServiceName: NamingConventions.toDataServiceClassName(entity.name),
        dataServiceInterfaceName: NamingConventions.toDataServiceInterfaceName(entity.name),
        createAssignments,
        updateAssignments,
        constructorArgs,
        mapAssertions,
        invalidCreateCases,
        invalidUpdateCases,
        hasInvalidCreateCases: invalidCreateCases.length > 0,
        hasInvalidUpdateCases: invalidUpdateCases.length > 0,
      };

      await this.writeRenderedTemplate(
        'tests-data-service.generated.cs.hbs',
        context,
        path.join(servicesDir, `${entity.name}DataServiceTests.cs`),
        true
      );

      await this.writeRenderedTemplate(
        'tests-validator.generated.cs.hbs',
        context,
        path.join(validatorsDir, `${entity.name}ValidatorTests.cs`),
        true
      );

      await this.writeRenderedTemplate(
        'tests-mapper.generated.cs.hbs',
        context,
        path.join(mappersDir, `${entity.name}MapperTests.cs`),
        true
      );

      await this.writeRenderedTemplate(
        'tests-search-service.generated.cs.hbs',
        context,
        path.join(servicesDir, `${entity.name}SearchServiceTests.cs`),
        true
      );
    }

    const instantiationContext = {
      baseNamespace,
      namespace,
      contextName,
      entities: model.entities
        .filter((e) => !e.isJunction)
        .map((e) => ({
          dataServiceInterfaceName: NamingConventions.toDataServiceInterfaceName(e.name),
          searchServiceName: `${e.name}SearchService`,
        })),
    };

    await this.writeRenderedTemplate(
      'tests-instantiation.generated.cs.hbs',
      instantiationContext,
      path.join(instantiationDir, `${contextName}InstantiationTests.cs`),
      true
    );
  }

  private buildDtoAssignments(entity: Entity, model: DataModel, dtoType: 'create' | 'update'): Assignment[] {
    return this.getDtoProperties(entity, dtoType).map((p) => ({
      name: p.name,
      value: this.getSampleValue(p, model),
    }));
  }

  private buildConstructorArgs(entity: Entity, model: DataModel): Array<{ value: string }> {
    const constructorProps = entity.properties.filter(
      (p) =>
        p.name !== 'Id' &&
        p.name !== 'CreatedAt' &&
        p.name !== 'UpdatedAt' &&
        !p.isCollection &&
        (!p.isForeignKey || p.required)
    );

    return constructorProps.map((p) => ({
      value: this.getSampleValue(p, model),
    }));
  }

  private buildMapAssertions(entity: Entity): Array<{ name: string }> {
    const properties = entity.properties.filter(
      (p) => !p.isCollection && !p.navigationProperty
    );

    return properties.map((p) => ({ name: p.name }));
  }

  private buildInvalidCases(entity: Entity, dtoType: 'create' | 'update'): InvalidCase[] {
    const invalidCases: InvalidCase[] = [];
    const properties = this.getDtoProperties(entity, dtoType);

    if (dtoType === 'update') {
      invalidCases.push({
        propertyName: 'Id',
        invalidValue: 'Guid.Empty',
      });
    }

    for (const prop of properties) {
      const propName = prop.name;

      if (prop.required && TypeMapper.isStringType(prop.type)) {
        invalidCases.push({
          propertyName: propName,
          invalidValue: 'string.Empty',
        });
      }

      if (prop.maxLength && TypeMapper.isStringType(prop.type)) {
        invalidCases.push({
          propertyName: propName,
          invalidValue: `new string('a', ${prop.maxLength + 1})`,
        });
      }

      if (prop.minLength && prop.minLength > 0 && TypeMapper.isStringType(prop.type)) {
        invalidCases.push({
          propertyName: propName,
          invalidValue: `new string('a', ${prop.minLength - 1})`,
        });
      }

      if (prop.validation?.min !== undefined && TypeMapper.isNumericType(prop.type)) {
        invalidCases.push({
          propertyName: propName,
          invalidValue: this.formatNumericExpression(prop.validation.min, prop.type, -1),
        });
      }

      if (prop.validation?.max !== undefined && TypeMapper.isNumericType(prop.type)) {
        invalidCases.push({
          propertyName: propName,
          invalidValue: this.formatNumericExpression(prop.validation.max, prop.type, 1),
        });
      }

      if (prop.validation?.regex) {
        invalidCases.push({
          propertyName: propName,
          invalidValue: '"invalid"',
        });
      }

      if (propName.toLowerCase().includes('email')) {
        invalidCases.push({
          propertyName: propName,
          invalidValue: '"invalid-email"',
        });
      }

      if (prop.type === 'Guid' && prop.required && prop.isForeignKey) {
        invalidCases.push({
          propertyName: propName,
          invalidValue: 'Guid.Empty',
        });
      }
    }

    return invalidCases;
  }

  private getDtoProperties(entity: Entity, dtoType: 'create' | 'update'): Property[] {
    return entity.properties.filter((p) => {
      if (p.isCollection || p.navigationProperty) {
        return false;
      }

      if (dtoType === 'create') {
        return p.name !== 'Id' && p.name !== 'CreatedAt' && p.name !== 'UpdatedAt';
      }

      return p.name !== 'Id' && p.name !== 'CreatedAt';
    });
  }

  private getSampleValue(property: Property, model: DataModel): string {
    if (property.enumType) {
      return this.getEnumSampleValue(property.enumType, model);
    }

    switch (property.type) {
      case 'string':
        return this.getStringSampleValue(property);
      case 'int':
      case 'long':
      case 'decimal':
      case 'double':
      case 'float':
        return this.getNumericSampleValue(property);
      case 'bool':
        return 'true';
      case 'DateTime':
        return 'DateTime.UtcNow';
      case 'DateTimeOffset':
        return 'DateTimeOffset.UtcNow';
      case 'Guid':
        return 'Guid.NewGuid()';
      case 'byte[]':
        return 'new byte[] { 1, 2, 3 }';
      default:
        return 'default!';
    }
  }

  private getStringSampleValue(property: Property): string {
    if (property.name.toLowerCase().includes('email')) {
      return '"user@example.com"';
    }

    if (property.validation?.regex) {
      return this.getRegexSampleValue(property.validation.regex);
    }

    const maxLength = property.maxLength ?? 10;
    const minLength = property.minLength ?? 1;
    const length = Math.min(Math.max(minLength, 1), maxLength);
    return `new string('a', ${length})`;
  }

  private getRegexSampleValue(regex: string): string {
    const normalized = regex.trim();

    if (normalized === '^[A-Z0-9-]+$') {
      return '"SKU-1"';
    }

    if (normalized === '^ORD-[0-9]{8}$') {
      return '"ORD-00000001"';
    }

    const sample = this.buildRegexSample(normalized);
    return `"${sample}"`;
  }

  private buildRegexSample(regex: string): string {
    let pattern = regex;
    if (pattern.startsWith('^')) {
      pattern = pattern.slice(1);
    }
    if (pattern.endsWith('$')) {
      pattern = pattern.slice(0, -1);
    }

    const alternationIndex = this.findTopLevelAlternation(pattern);
    if (alternationIndex >= 0) {
      pattern = pattern.slice(0, alternationIndex);
    }

    let result = '';

    for (let i = 0; i < pattern.length; i++) {
      const token = pattern[i];
      let sample = '';

      if (token === '\\') {
        i += 1;
        if (i >= pattern.length) {
          break;
        }
        sample = this.sampleForEscape(pattern[i]);
      } else if (token === '[') {
        const end = pattern.indexOf(']', i + 1);
        if (end === -1) {
          break;
        }
        const classContent = pattern.slice(i + 1, end);
        sample = this.sampleForCharClass(classContent);
        i = end;
      } else if (token === '(') {
        const end = this.findMatchingParen(pattern, i);
        if (end === -1) {
          break;
        }
        const group = pattern.slice(i + 1, end);
        sample = this.buildRegexSample(group);
        i = end;
      } else if (token === '^' || token === '$') {
        continue;
      } else {
        sample = token;
      }

      let repeat = 1;
      if (i + 1 < pattern.length) {
        const next = pattern[i + 1];
        if (next === '+' || next === '*' || next === '?') {
          repeat = 1;
          i += 1;
        } else if (next === '{') {
          const end = pattern.indexOf('}', i + 1);
          if (end !== -1) {
            const range = pattern.slice(i + 2, end);
            const min = parseInt(range.split(',')[0], 10);
            repeat = Number.isNaN(min) ? 1 : Math.max(min, 1);
            i = end;
          }
        }
      }

      result += sample.repeat(repeat);
    }

    return result.length > 0 ? result : 'REGEX-OK';
  }

  private findTopLevelAlternation(pattern: string): number {
    let depth = 0;
    for (let i = 0; i < pattern.length; i++) {
      const token = pattern[i];
      if (token === '\\') {
        i += 1;
        continue;
      }
      if (token === '(') {
        depth += 1;
      } else if (token === ')') {
        depth = Math.max(depth - 1, 0);
      } else if (token === '|' && depth === 0) {
        return i;
      }
    }
    return -1;
  }

  private findMatchingParen(pattern: string, start: number): number {
    let depth = 0;
    for (let i = start; i < pattern.length; i++) {
      const token = pattern[i];
      if (token === '\\') {
        i += 1;
        continue;
      }
      if (token === '(') {
        depth += 1;
      } else if (token === ')') {
        depth -= 1;
        if (depth === 0) {
          return i;
        }
      }
    }
    return -1;
  }

  private sampleForEscape(token: string): string {
    switch (token) {
      case 'd':
        return '0';
      case 'w':
        return 'a';
      case 's':
        return ' ';
      default:
        return token;
    }
  }

  private sampleForCharClass(content: string): string {
    if (content.includes('A-Z')) {
      return 'A';
    }
    if (content.includes('a-z')) {
      return 'a';
    }
    if (content.includes('0-9')) {
      return '0';
    }

    for (const ch of content) {
      if (/[A-Za-z0-9]/.test(ch)) {
        return ch;
      }
    }

    return 'A';
  }

  private getNumericSampleValue(property: Property): string {
    const min = property.validation?.min;
    const max = property.validation?.max;

    let value = 1;
    if (min !== undefined) {
      value = min;
    } else if (max !== undefined) {
      value = max - 1;
    }

    return this.formatNumericLiteral(value, property.type);
  }

  private formatNumericExpression(base: number, type: string, delta: number): string {
    const baseLiteral = this.formatNumericLiteral(base, type);
    const deltaLiteral = this.formatNumericLiteral(Math.abs(delta), type);
    const op = delta >= 0 ? '+' : '-';
    return `(${baseLiteral} ${op} ${deltaLiteral})`;
  }

  private formatNumericLiteral(value: number, type: string): string {
    switch (type) {
      case 'decimal':
        return `${value}m`;
      case 'float':
        return `${value}f`;
      case 'double':
        return `${value}d`;
      case 'long':
        return `${Math.trunc(value)}L`;
      default:
        return `${Math.trunc(value)}`;
    }
  }

  private getEnumSampleValue(enumType: string, model: DataModel): string {
    const enumDef = model.enums?.find((e) => e.name === enumType);
    if (!enumDef || enumDef.values.length === 0) {
      return `default(${enumType})`;
    }

    return `${enumType}.${enumDef.values[0].name}`;
  }
}
