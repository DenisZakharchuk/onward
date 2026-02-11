/**
 * Validator generator - creates IValidator implementations for Create/Update DTOs
 */

import { BaseGenerator } from './BaseGenerator';
import { DataModel, Entity, Property } from '../models/DataModel';
import { TypeMapper } from '../utils/TypeMapper';
import * as path from 'path';

export class ValidatorGenerator extends BaseGenerator {
  async generate(model: DataModel): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    const baseNamespace = this.metadata?.baseNamespace || 'Inventorization';

    const validatorsDir = `${baseNamespace}.${contextName}.Domain/Validators`;

    for (const entity of model.entities) {
      // Skip junction entities
      if (entity.isJunction) {
        continue;
      }

      await this.generateCreateValidator(entity, validatorsDir, namespace, baseNamespace);
      await this.generateUpdateValidator(entity, validatorsDir, namespace, baseNamespace);
    }
  }

  private async generateCreateValidator(
    entity: Entity,
    validatorsDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const validationRules = this.getValidationRules(entity, 'create');

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      hasDependencies: false,
      dependencies: [],
      validationRules,
    };

    const filePath = path.join(validatorsDir, `Create${entity.name}Validator.cs`);
    await this.writeRenderedTemplate('create-validator.generated.cs.hbs', context, filePath, true);
  }

  private async generateUpdateValidator(
    entity: Entity,
    validatorsDir: string,
    namespace: string,
    baseNamespace: string
  ): Promise<void> {
    const validationRules = this.getValidationRules(entity, 'update');

    const context = {
      baseNamespace,
      namespace,
      entityName: entity.name,
      hasDependencies: false,
      dependencies: [],
      validationRules,
    };

    const filePath = path.join(validatorsDir, `Update${entity.name}Validator.cs`);
    await this.writeRenderedTemplate('update-validator.generated.cs.hbs', context, filePath, true);
  }

  private getValidationRules(entity: Entity, dtoType: 'create' | 'update'): string[] {
    const rules: string[] = [];

    // Get properties to validate based on DTO type
    const properties = this.getPropertiesToValidate(entity, dtoType);

    for (const prop of properties) {
      const propName = prop.name;

      // Required validation for strings
      if (prop.required && TypeMapper.isStringType(prop.type)) {
        rules.push(
          `if (string.IsNullOrWhiteSpace(dto.${propName}))\n` +
            `    errors.Add("${propName} is required");`
        );
      }

      // MaxLength validation
      if (prop.maxLength && TypeMapper.isStringType(prop.type)) {
        rules.push(
          `if (!string.IsNullOrEmpty(dto.${propName}) && dto.${propName}.Length > ${prop.maxLength})\n` +
            `    errors.Add("${propName} cannot exceed ${prop.maxLength} characters");`
        );
      }

      // MinLength validation
      if (prop.minLength && TypeMapper.isStringType(prop.type)) {
        rules.push(
          `if (!string.IsNullOrEmpty(dto.${propName}) && dto.${propName}.Length < ${prop.minLength})\n` +
            `    errors.Add("${propName} must be at least ${prop.minLength} characters");`
        );
      }

      // Min value validation for numeric types
      if (prop.validation?.min !== undefined && TypeMapper.isNumericType(prop.type)) {
        rules.push(
          `if (dto.${propName} < ${prop.validation.min})\n` +
            `    errors.Add("${propName} must be at least ${prop.validation.min}");`
        );
      }

      // Max value validation for numeric types
      if (prop.validation?.max !== undefined && TypeMapper.isNumericType(prop.type)) {
        rules.push(
          `if (dto.${propName} > ${prop.validation.max})\n` +
            `    errors.Add("${propName} cannot exceed ${prop.validation.max}");`
        );
      }

      // Regex validation
      if (prop.validation?.regex) {
        rules.push(
          `if (!string.IsNullOrEmpty(dto.${propName}) && !System.Text.RegularExpressions.Regex.IsMatch(dto.${propName}, @"${prop.validation.regex}"))\n` +
            `    errors.Add("${propName} format is invalid");`
        );
      }

      // Email validation (check property name for common patterns)
      if (propName.toLowerCase().includes('email')) {
        rules.push(
          `if (!string.IsNullOrEmpty(dto.${propName}) && !System.Net.Mail.MailAddress.TryCreate(dto.${propName}, out _))\n` +
            `    errors.Add("${propName} must be a valid email address");`
        );
      }

      // Guid validation (not empty)
      if (prop.type === 'Guid' && prop.required && prop.isForeignKey) {
        rules.push(
          `if (dto.${propName} == Guid.Empty)\n` +
            `    errors.Add("${propName} is required");`
        );
      }
    }

    return rules;
  }

  private getPropertiesToValidate(entity: Entity, dtoType: 'create' | 'update'): Property[] {
    // Filter out properties that don't belong in DTOs
    return entity.properties.filter((p) => {
      // Skip collections and navigation properties
      if (p.isCollection || p.navigationProperty) {
        return false;
      }

      // Skip Id, CreatedAt, UpdatedAt for Create DTO
      if (dtoType === 'create') {
        return p.name !== 'Id' && p.name !== 'CreatedAt' && p.name !== 'UpdatedAt';
      }

      // For Update DTO, include all except Id and system fields
      return p.name !== 'Id' && p.name !== 'CreatedAt';
    });
  }
}
