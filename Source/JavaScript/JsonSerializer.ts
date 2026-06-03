// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConceptAs } from './ConceptAs';
import { Constructor } from './Constructor';
import { DerivedType } from './DerivedType';
import { Field } from './Field';
import { Fields } from './Fields';
import { Guid } from './Guid';
import { TimeSpan } from './TimeSpan';
import { ValueMap } from './ValueMap';

/* eslint-disable @typescript-eslint/no-explicit-any */

type typeSerializer = (value: any) => any;

const typeConverters: Map<Constructor, typeSerializer> = new Map<Constructor, typeSerializer>([
    [Number, (value: number) => value],
    [String, (value: string) => value],
    [Boolean, (value: boolean) => value],
    [Date, (value: Date) => value.toISOString()],
    [Guid, (value: Guid) => value?.toString() ?? ''],
    [TimeSpan, (value: TimeSpan) => value?.toString() ?? ''],
    [ValueMap, (value: ValueMap<any, any>) => {
        const converted: any = {};
        for (const [key, mapValue] of value.entries()) {
            converted[serializeMapKey(key)] = convertTypesOnInstance(mapValue);
        }

        return converted;
    }]
]);

const typeSerializers: Map<Constructor, typeSerializer> = new Map<Constructor, typeSerializer>([
    [Number, (value: any) => value],
    [String, (value: any) => value],
    [Boolean, (value: any) => value],
    [Date, (value: any) => new Date(value)],
    [Guid, (value: any) => Guid.parse(value.toString())],
    [TimeSpan, (value: any) => TimeSpan.parse(value.toString())],
]);

/**
 * Checks if a constructor is a ConceptAs type.
 * @param {Constructor} type The constructor to check.
 * @returns {boolean} True if the type extends ConceptAs.
 */
const isConceptAs = (type: Constructor): boolean => {
    if (!type || !type.prototype) return false;
    
    // Check if the prototype chain includes ConceptAs
    let proto = type.prototype;
    while (proto) {
        // Check if this prototype is ConceptAs.prototype
        if (proto === ConceptAs.prototype) {
            return true;
        }
        proto = Object.getPrototypeOf(proto);
    }
    return false;
};

const serializeValueForType = (type: Constructor, value: any) => {
    if (!value) return value;

    // If it's a ConceptAs instance, unwrap it and serialize the inner value recursively
    // This follows the C# pattern: recognize as concept, unwrap, then call serializer
    if (value instanceof ConceptAs) {
        const innerValue = value.value;
        // Recursively serialize the inner value to handle complex types
        // Use .constructor directly which works reliably for both primitives and objects
        return serializeValueForType(innerValue.constructor, innerValue);
    }

    if (typeConverters.has(type)) {
        return typeConverters.get(type)!(value);
    } else {
        return convertTypesOnInstance(value);
    }
};

const deserializeValueFromType = (type: Constructor, value: any) => {
    // If it's a ConceptAs type, instantiate it with the value
    if (isConceptAs(type)) {
        return new type(value);
    }
    
    if (typeSerializers.has(type)) {
        return typeSerializers.get(type)!(value);
    } else {
        return JsonSerializer.deserialize(type, JSON.stringify(value));
    }
};

const deserializeValueFromField = (field: Field, value: any) => {
    if (field.type === ValueMap) {
        return deserializeValueMapFromField(field, value);
    }

    // If it's a ConceptAs type, instantiate it with the value
    if (isConceptAs(field.type)) {
        return new field.type(value);
    }

    if (typeSerializers.has(field.type)) {
        return typeSerializers.get(field.type)!(value);
    } else {
        let type = field.type;
        if (value[JsonSerializer.DerivedTypeIdProperty]) {
            const derivedTypeId = value[JsonSerializer.DerivedTypeIdProperty];
            const candidates = [...field.derivatives, ...DerivedType.getDerivedTypesFor(field.type)];
            type = candidates.find(_ => DerivedType.get(_) == derivedTypeId) || type;
        }

        return JsonSerializer.deserialize(type, JSON.stringify(value));
    }
};

const serializeMapKey = (key: any): string => {
    if (key === undefined || key === null) {
        return '';
    }

    if (typeof key === 'string') {
        return key;
    }

    if (typeof key === 'number' || typeof key === 'boolean' || typeof key === 'bigint') {
        return key.toString();
    }

    if (key instanceof Date) {
        return key.toISOString();
    }

    return JsonSerializer.serialize(key);
};

const deserializeMapKey = (keyType: Constructor, key: string): any => {
    if (keyType === String) {
        return key;
    }

    if (keyType === Number) {
        return Number(key);
    }

    if (keyType === Boolean) {
        return key.toLowerCase() === 'true';
    }

    if (keyType === Date) {
        return new Date(key);
    }

    if (typeSerializers.has(keyType)) {
        return typeSerializers.get(keyType)!(key);
    }

    return JsonSerializer.deserialize(keyType, key);
};

const deserializeMapValue = (valueType: Constructor | undefined, value: any): any => {
    if (!valueType) {
        return value;
    }

    if (typeSerializers.has(valueType)) {
        return typeSerializers.get(valueType)!(value);
    }

    return JsonSerializer.deserialize(valueType, JSON.stringify(value));
};

const deserializeValueMapFromField = (field: Field, value: any): ValueMap<any, any> => {
    const valueMap = new ValueMap<any, any>();
    const keyType = field.genericArguments[0];
    const valueType = field.genericArguments[1];

    if (!value || !keyType) {
        return valueMap;
    }

    for (const key of Object.keys(value)) {
        const deserializedKey = deserializeMapKey(keyType, key);
        const deserializedValue = deserializeMapValue(valueType, value[key]);
        valueMap.set(deserializedKey, deserializedValue);
    }

    return valueMap;
};

const convertTypesOnInstance = (instance: any) => {
    if (typeConverters.has(instance.constructor)) {
        return serializeValueForType(instance.constructor, instance);
    }

    const properties = Object.getOwnPropertyNames(instance);
    const converted: any = {};

    const derivedTypeId = DerivedType.get(instance.constructor);
    if (derivedTypeId) {
        converted[JsonSerializer.DerivedTypeIdProperty] = derivedTypeId;
    }
    properties.forEach(property => {
        let value = instance[property];
        if (value !== undefined) {
            if (Array.isArray(value)) {
                value = value.map(_ => convertTypesOnInstance(_));
            } else {
                value = serializeValueForType(value.__proto__.constructor, value);
            }
        }

        converted[property] = value;
    });

    return converted;
};

/**
 * Represents a serializer for JSON.
 */
export class JsonSerializer {
    static readonly DerivedTypeIdProperty: string = "_derivedTypeId";

    /**
     * Serialize with strong type information.
     * @param {*} value The value to serialize.
     * @returns A JSON string.
     */
    static serialize(value: any): string {
        const converted = convertTypesOnInstance(value);
        return JSON.stringify(converted);
    }

    /**
     * Deserialize a JSON string to the specific type.
     * @param {Constructor} targetType Type to deserialize to.
     * @param {string} json Actual JSON to deserialize.
     * @returns An instance of the target type.
     */
    static deserialize<TResult extends object>(targetType: Constructor<TResult>, json: string): TResult {
        const parsed = JSON.parse(json);
        return this.deserializeFromInstance<TResult>(targetType, parsed);
    }

    /**
     * Deserialize a array JSON string to an array of the specific instance type.
     * @param {Constructor} targetType Type to deserialize to.
     * @param {string} json Actual JSON to deserialize.
     * @returns An array of instances of the target type.
     */
    static deserializeArray<TResult extends object>(targetType: Constructor<TResult>, json: string): TResult[] {
        const parsed = JSON.parse(json);
        return this.deserializeArrayFromInstance(targetType, parsed);
    }

    /**
     * Deserialize an any instance to a specific instance type.
     * @param {Constructor} targetType Type to deserialize to.
     * @param {*} instance Actual instance to deserialize.
     * @returns An instance of the target type.
     */
    static deserializeFromInstance<TResult extends object>(targetType: Constructor<TResult>, instance: any): TResult {
        const fields = Fields.getFieldsForType(targetType as Constructor);

        if (typeSerializers.has(targetType)) {
            return deserializeValueFromType(targetType, instance);
        }

        const deserialized = new targetType();
        for (const field of fields) {
            let value = instance[field.name];
            if (value) {
                if (field.enumerable) {
                    value = value.map(_ => deserializeValueFromField(field, _));
                } else {
                    value = deserializeValueFromField(field, value);
                }
            }

            deserialized[field.name] = value;
        }

        if ((targetType as Constructor) == Object) {
            const objectFields = Object.keys(instance).filter((value) => {
                return !fields.some(_ => _.name == value);
            });

            for (const field of objectFields) {
                deserialized[field] = instance[field];
            }
        }

        return deserialized;
    }

    /**
     * Deserialize an array of any instances to an array of specific instance types.
     * @param {Constructor} targetType Type to deserialize to.
     * @param {instances} instances Actual instances to deserialize.
     * @returns An array of instances of the target type.
     */
    static deserializeArrayFromInstance<TResult extends object>(targetType: Constructor<TResult>, instances: any): TResult[] {
        const deserialized: TResult[] = [];

        for (const instance of instances) {
            deserialized.push(this.deserializeFromInstance<TResult>(targetType, instance));
        }

        return deserialized;
    }
}
