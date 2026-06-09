// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { ValueMap } from '../ValueMap';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */

/**
 * JSON converter for ValueMap type.
 * 
 * Note: ValueMap deserialization is handled separately by JsonSerializer.deserializeValueMapFromField
 * based on field metadata to properly deserialize keys and values according to their types.
 * The read() method is not used directly during normal deserialization.
 */
export class ValueMapJsonConverter extends JsonConverter<ValueMap<any, any>> {
    // Circular dependency workaround - these will be injected by JsonSerializer
    private static serializeMapKey: ((key: any) => string) | null = null;
    private static convertTypesOnInstance: ((instance: any) => any) | null = null;

    /**
     * Sets the required helper functions from JsonSerializer to avoid circular dependencies.
     */
    static setHelpers(serializeMapKey: (key: any) => string, convertTypesOnInstance: (instance: any) => any): void {
        ValueMapJsonConverter.serializeMapKey = serializeMapKey;
        ValueMapJsonConverter.convertTypesOnInstance = convertTypesOnInstance;
    }

    /** @inheritdoc */
    get type(): Constructor<ValueMap<any, any>> {
        return ValueMap;
    }

    /** @inheritdoc */
    read(_value: any): ValueMap<any, any> {
        // ValueMap deserialization is handled separately by JsonSerializer.deserializeValueMapFromField
        // based on field metadata to properly deserialize keys and values according to their types.
        // This method should not be called directly during normal deserialization.
        const valueMap = new ValueMap<any, any>();
        return valueMap;
    }

    /** @inheritdoc */
    write(value: ValueMap<any, any>): any {
        if (!ValueMapJsonConverter.serializeMapKey || !ValueMapJsonConverter.convertTypesOnInstance) {
            throw new Error('ValueMapJsonConverter helpers not initialized. Call setHelpers() first.');
        }

        const converted: any = {};
        for (const [key, mapValue] of value.entries()) {
            converted[ValueMapJsonConverter.serializeMapKey(key)] = ValueMapJsonConverter.convertTypesOnInstance(mapValue);
        }
        return converted;
    }
}
