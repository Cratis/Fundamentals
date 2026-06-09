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
 * Note: This converter is NOT registered in the JsonSerializer converters list.
 * ValueMap serialization is handled as a special case in JsonSerializer.serializeValueForType
 * to properly support nested type conversions through convertTypesOnInstance. This avoids
 * circular dependencies and ensures that map values are recursively serialized correctly.
 * 
 * This class exists primarily for consistency with the converter pattern and for potential
 * future use if ValueMap serialization logic needs to be refactored.
 */
export class ValueMapJsonConverter extends JsonConverter<ValueMap<any, any>> {
    /** @inheritdoc */
    get type(): Constructor<ValueMap<any, any>> {
        return ValueMap;
    }

    /** @inheritdoc */
    read(_value: any): ValueMap<any, any> {
        // ValueMap deserialization is handled separately by JsonSerializer.deserializeValueMapFromField
        // based on field metadata to properly deserialize keys and values according to their types.
        // This method should not be called directly.
        const valueMap = new ValueMap<any, any>();
        return valueMap;
    }

    /** @inheritdoc */
    write(_value: any): any {
        // ValueMap serialization is handled by JsonSerializer.serializeValueForType as a special case
        // to properly handle nested type conversions. This method should not be called directly.
        return null;
    }
}
