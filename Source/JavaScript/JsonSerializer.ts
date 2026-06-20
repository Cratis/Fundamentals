// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConceptAs } from './ConceptAs';
import { Constructor } from './Constructor';
import { DerivedType } from './DerivedType';
import { Field } from './Field';
import { Fields } from './Fields';
import { ValueMap } from './ValueMap';
import { 
    JsonConverter, 
    DateJsonConverter, 
    GuidJsonConverter, 
    TimeSpanJsonConverter, 
    PointJsonConverter,
    LineStringJsonConverter,
    PolygonJsonConverter,
    ValueMapJsonConverter
} from './json';

/* eslint-disable @typescript-eslint/no-explicit-any */

type typeSerializer = (value: any) => any;

// Initialize converters
const converters: JsonConverter[] = [
    new DateJsonConverter(),
    new GuidJsonConverter(),
    new TimeSpanJsonConverter(),
    new PointJsonConverter(),
    new LineStringJsonConverter(),
    new PolygonJsonConverter(),
    new ValueMapJsonConverter()
];

// Build converter maps from the converters
const typeConverters: Map<Constructor, JsonConverter> = new Map<Constructor, JsonConverter>();
const typeSerializers: Map<Constructor, JsonConverter> = new Map<Constructor, JsonConverter>();

for (const converter of converters) {
    typeConverters.set(converter.type, converter);
    typeSerializers.set(converter.type, converter);
}

// Add primitive type converters that don't need a full JsonConverter class
const primitiveConverters: Map<Constructor, typeSerializer> = new Map<Constructor, typeSerializer>([
    [Number, (value: number) => value],
    [String, (value: string) => value],
    [Boolean, (value: boolean) => value]
]);

const primitiveSerializers: Map<Constructor, typeSerializer> = new Map<Constructor, typeSerializer>([
    [Number, (value: any) => value],
    [String, (value: any) => value],
    [Boolean, (value: any) => value]
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

    // Check if there's a registered converter
    if (typeConverters.has(type)) {
        const converter = typeConverters.get(type)!;
        return converter.write(value);
    }
    
    // Check primitive converters
    if (primitiveConverters.has(type)) {
        return primitiveConverters.get(type)!(value);
    }
    
    return convertTypesOnInstance(value);
};

const deserializeValueFromType = (type: Constructor, value: any) => {
    // If it's a ConceptAs type, instantiate it with the value
    if (isConceptAs(type)) {
        return new type(value);
    }
    
    // Check if there's a registered converter
    if (typeSerializers.has(type)) {
        const converter = typeSerializers.get(type)!;
        return converter.read(value);
    }
    
    // Check primitive serializers
    if (primitiveSerializers.has(type)) {
        return primitiveSerializers.get(type)!(value);
    }
    
    return JsonSerializer.deserialize(type, JSON.stringify(value));
};

const deserializeValueFromField = (field: Field, value: any) => {
    if (field.type === ValueMap) {
        return deserializeValueMapFromField(field, value);
    }

    // If it's a ConceptAs type, instantiate it with the value
    if (isConceptAs(field.type)) {
        return new field.type(value);
    }

    // Check if there's a registered converter
    if (typeSerializers.has(field.type)) {
        const converter = typeSerializers.get(field.type)!;
        return converter.read(value);
    }
    
    // Check primitive serializers
    if (primitiveSerializers.has(field.type)) {
        return primitiveSerializers.get(field.type)!(value);
    }
    
    let type = field.type;
    if (value[JsonSerializer.DerivedTypeIdProperty]) {
        const derivedTypeId = value[JsonSerializer.DerivedTypeIdProperty];
        const candidates = [...field.derivatives, ...DerivedType.getDerivedTypesFor(field.type)];
        type = candidates.find(_ => DerivedType.get(_) == derivedTypeId) || type;
    }

    return JsonSerializer.deserialize(type, JSON.stringify(value));
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

    // Check if there's a converter for this type
    if (typeSerializers.has(keyType)) {
        const converter = typeSerializers.get(keyType)!;
        return converter.read(key);
    }

    return JsonSerializer.deserialize(keyType, key);
};

const deserializeMapValue = (valueType: Constructor | undefined, value: any): any => {
    if (!valueType) {
        return value;
    }

    // Check if there's a converter for this type
    if (typeSerializers.has(valueType)) {
        const converter = typeSerializers.get(valueType)!;
        return converter.read(value);
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
    // Check if there's a converter for this type
    if (typeConverters.has(instance.constructor)) {
        return serializeValueForType(instance.constructor, instance);
    }
    
    // Check primitive converters
    if (primitiveConverters.has(instance.constructor)) {
        return primitiveConverters.get(instance.constructor)!(instance);
    }

    const properties = Object.getOwnPropertyNames(instance);
    const converted: any = {};

    // Emit the polymorphic type discriminator, preferring the registered type for the instance's
    // constructor and falling back to a discriminator preserved on the instance itself (e.g. a value
    // that was deserialized before its concrete type could be resolved). This keeps '_derivedTypeId'
    // flowing through serialization transparently — the caller never has to manage it — and is the
    // mirror of preserving it on deserialization.
    const derivedTypeId = DerivedType.get(instance.constructor) ?? instance[JsonSerializer.DerivedTypeIdProperty];
    if (derivedTypeId) {
        converted[JsonSerializer.DerivedTypeIdProperty] = derivedTypeId;
    }
    properties.forEach(property => {
        // The discriminator is handled above as a meta-property; never treat it as a data field.
        if (property === JsonSerializer.DerivedTypeIdProperty) return;
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

// Initialize ValueMapJsonConverter with helper functions to avoid circular dependencies
// Must be done after serializeMapKey and convertTypesOnInstance are defined
ValueMapJsonConverter.setHelpers(serializeMapKey, convertTypesOnInstance);

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

        // Preserve the polymorphic type discriminator so it is never lost across (de)serialization
        // layers. '_derivedTypeId' is not a declared field, so it is otherwise dropped here — and a
        // derived value whose concrete type could not be resolved (e.g. its type was not yet
        // registered) would then have no discriminator left to re-resolve or round-trip with, and
        // would serialize back with no type information at all.
        const derivedTypeId = instance[JsonSerializer.DerivedTypeIdProperty];
        if (derivedTypeId !== undefined && derivedTypeId !== null) {
            (deserialized as Record<string, unknown>)[JsonSerializer.DerivedTypeIdProperty] = derivedTypeId;
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
