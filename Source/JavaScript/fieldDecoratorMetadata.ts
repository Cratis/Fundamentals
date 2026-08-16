// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import './reflection';
import { Constructor } from './Constructor';
import { Field } from './Field';

const fieldsMetadataKey = Symbol.for('@cratis/fundamentals.fields');

/**
 * Adds a field to the metadata object supplied to a standard decorator.
 * @param metadata The class metadata object supplied by the decorator runtime.
 * @param field The field to add.
 */
export function addFieldToDecoratorMetadata(metadata: Record<PropertyKey, unknown>, field: Field): void {
    const fields = Reflect.getOwnMetadata(fieldsMetadataKey, metadata) as Map<string, Field> | undefined ?? new Map<string, Field>();
    fields.set(field.name, field);
    Reflect.defineMetadata(fieldsMetadataKey, fields, metadata);
}

/**
 * Gets the fields stored in the standard decorator metadata owned by a type.
 * @param target The type to read metadata from.
 * @returns The fields declared by the type through standard decorators.
 */
export function getOwnDecoratorFieldsForType(target: Constructor): Field[] {
    if (!Object.prototype.hasOwnProperty.call(target, Symbol.metadata)) return [];

    const metadata = (target as Constructor & { [Symbol.metadata]?: Record<PropertyKey, unknown> | null })[Symbol.metadata];
    if (!metadata) return [];

    const fields = Reflect.getOwnMetadata(fieldsMetadataKey, metadata) as Map<string, Field> | undefined;
    return fields ? [...fields.values()] : [];
}
