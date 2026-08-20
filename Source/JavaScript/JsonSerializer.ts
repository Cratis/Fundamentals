// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from './Constructor';
import { DerivedType } from './DerivedType';
import { registerModuleInstance } from './duplicateInstanceGuard';
import { Field } from './Field';
import { Fields } from './Fields';
import { conceptAsTypeKey, declaredTypeKey, typeKeyOf, valueMapTypeKey } from './typeKey';
import { ValueMap } from './ValueMap';
import { 
    JsonConverter, 
    DateJsonConverter, 
    DateOnlyJsonConverter,
    GuidJsonConverter, 
    TimeOnlyJsonConverter,
    TimeSpanJsonConverter, 
    PointJsonConverter,
    LineStringJsonConverter,
    PolygonJsonConverter,
    ValueMapJsonConverter
} from './json';

/* eslint-disable @typescript-eslint/no-explicit-any */

type typeSerializer = (value: any) => any;

// A second copy of this package in the same realm builds its own registry here, and brings its own
// class objects. Convertible values still cross between the copies, because a type is recognized by
// the key it declares - but a converter registered on one copy reaches only that one, and a version
// pinned at the top level never reaches a nested copy. Announce it where the state that cannot be
// shared is built.
registerModuleInstance();

// Initialize converters
const converters: JsonConverter[] = [
    new DateJsonConverter(),
    new DateOnlyJsonConverter(),
    new GuidJsonConverter(),
    new TimeOnlyJsonConverter(),
    new TimeSpanJsonConverter(),
    new PointJsonConverter(),
    new LineStringJsonConverter(),
    new PolygonJsonConverter(),
    new ValueMapJsonConverter()
];

// Converters are found by the key a type declares, and only by the constructor when it declares none.
// Keying on the constructor alone is what made a second copy of this package silently fatal: each copy
// brings its own Guid, ConceptAs and ValueMap class objects, so neither recognized the other's. A key
// crosses that boundary; a class object does not. A consumer's own type has no key and is found by
// constructor, which is exact and needs nothing to cross.
const keyedConverters: Map<string, JsonConverter> = new Map<string, JsonConverter>();
const typeConverters: Map<Constructor, JsonConverter> = new Map<Constructor, JsonConverter>();

const registerConverterFor = (converter: JsonConverter) => {
    const key = declaredTypeKey(converter.type);
    if (key !== undefined) {
        keyedConverters.set(key, converter);
    }

    typeConverters.set(converter.type, converter);
};

converters.forEach(registerConverterFor);

/**
 * Finds the converter registered for a type, if any.
 * @param {Constructor} type The type to convert.
 * @returns {JsonConverter | undefined} The converter, or undefined when the type has none.
 */
const converterFor = (type: Constructor | undefined): JsonConverter | undefined => {
    if (!type) return undefined;

    const key = declaredTypeKey(type);
    const keyed = key === undefined ? undefined : keyedConverters.get(key);
    return keyed ?? typeConverters.get(type);
};

/**
 * Checks whether a type is a ValueMap, including one from another copy of this package.
 * @param {Constructor} type The type to check.
 * @returns {boolean} True when the type is a ValueMap.
 */
const isValueMap = (type: Constructor | undefined): boolean => declaredTypeKey(type) === valueMapTypeKey;

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
 * @remarks
 * Reads the key rather than walking the prototype chain looking for this copy's ConceptAs. The key is a
 * static and a static is inherited, so every type deriving from ConceptAs carries it - including one
 * deriving from another copy's ConceptAs, which a prototype walk could never recognize.
 */
const isConceptAs = (type: Constructor): boolean => typeKeyOf(type) === conceptAsTypeKey;

const serializeValueForType = (type: Constructor, value: any) => {
    if (!value) return value;

    // If it's a ConceptAs instance, unwrap it and serialize the inner value recursively
    // This follows the C# pattern: recognize as concept, unwrap, then call serializer
    if (isConceptAs(value.constructor)) {
        const innerValue = value.value;
        // Recursively serialize the inner value to handle complex types
        // Use .constructor directly which works reliably for both primitives and objects
        return serializeValueForType(innerValue.constructor, innerValue);
    }

    // Check if there's a registered converter
    const converter = converterFor(type);
    if (converter) {
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
    const converter = converterFor(type);
    if (converter) {
        return converter.read(value);
    }
    
    // Check primitive serializers
    if (primitiveSerializers.has(type)) {
        return primitiveSerializers.get(type)!(value);
    }
    
    return JsonSerializer.deserialize(type, JSON.stringify(value));
};

const deserializeValueFromField = (field: Field, value: any) => {
    if (isValueMap(field.type)) {
        return deserializeValueMapFromField(field, value);
    }

    // If it's a ConceptAs type, instantiate it with the value
    if (isConceptAs(field.type)) {
        return new field.type(value);
    }

    // Check if there's a registered converter
    const converter = converterFor(field.type);
    if (converter) {
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
    const converter = converterFor(keyType);
    if (converter) {
        return converter.read(key);
    }

    return JsonSerializer.deserialize(keyType, key);
};

const deserializeMapValue = (valueType: Constructor | undefined, value: any): any => {
    if (!valueType) {
        return value;
    }

    // A map value is an ordinary value of its declared type, so read it the way any other value of that
    // type is read. Resolving it here instead left out the concept and primitive cases, and the
    // primitive one lost data without a sound: a string value went through JsonSerializer.deserialize,
    // which built an empty String object because a primitive has no fields to populate.
    return deserializeValueFromType(valueType, value);
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
    // A concept unwraps to its underlying value wherever it is reached, not only when it is reached
    // through a declared field. An array element and a map value arrive here rather than at
    // serializeValueForType, and without this they were written as the object a concept happens to be -
    // {"value": ...} where the receiver declared the underlying type.
    if (isConceptAs(instance.constructor)) {
        return serializeValueForType(instance.constructor as Constructor, instance);
    }

    // Check if there's a converter for this type
    if (converterFor(instance.constructor)) {
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
     * Register a converter for the type it declares through its `type` property.
     * @param {JsonConverter} converter The converter to register.
     * @remarks
     * The converter is used for both directions - `read` on the way in, `write` on the way out - and
     * takes the place of any converter already registered for the same type, including the built-in
     * ones. Replacing a built-in changes the payloads the application produces and consumes, which is
     * the caller's to decide; nothing here second-guesses it.
     *
     * Two shapes resolve ahead of the converters, so a converter registered for them is not reached:
     *
     * - A type deriving from `ConceptAs` is unwrapped to its underlying value in both directions, and
     *   converted as that value's type. Register for the underlying type to reach a concept.
     * - A `ValueMap` is read back from the declaring field's generic arguments rather than through a
     *   converter. Note the asymmetry: writing a `ValueMap` *does* go through the registered converter,
     *   so replacing that one changes only the outbound half.
     */
    static registerConverter(converter: JsonConverter): void {
        registerConverterFor(converter);
    }

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

        if (converterFor(targetType)) {
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
            } else if (field.enumerable && value === undefined) {
                // A collection that is absent from the payload deserializes to an empty one rather than to
                // undefined. The declared type says the field is an array, and a producer that leaves an
                // empty collection out - which the Chronicle sink does deliberately, so that a parallel
                // replay cannot overwrite children a sibling event already wrote - would otherwise hand back
                // a value contradicting its own type, putting a null guard on every reader of every
                // collection. An explicit null is left alone: that is the producer saying "no collection"
                // rather than "an empty one".
                value = [];
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
