// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * Base class for JSON converters that handle serialization and deserialization of specific types.
 * Similar to System.Text.Json.Serialization.JsonConverter<T>.
 */
export abstract class JsonConverter<T = any> {
    /**
     * Gets the type that this converter handles.
     */
    abstract get type(): Constructor<T>;

    /**
     * Determines whether the converter can convert the specified type.
     * @param {Constructor} typeToConvert - The type to check.
     * @returns {boolean} True if the converter can convert the type; otherwise, false.
     */
    canConvert(typeToConvert: Constructor): boolean {
        return typeToConvert === this.type;
    }

    /**
     * Reads and converts the JSON to the target type.
     * @param {any} value - The value to deserialize.
     * @returns {T} The deserialized value.
     */
    abstract read(value: any): T;

    /**
     * Writes the value as JSON.
     * @param {T} value - The value to serialize.
     * @returns {any} The serialized value.
     */
    abstract write(value: T): any;
}
