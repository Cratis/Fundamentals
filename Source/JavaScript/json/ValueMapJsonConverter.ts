// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { ValueMap } from '../ValueMap';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */

/**
 * JSON converter for ValueMap type.
 * Note: Full serialization logic is handled in JsonSerializer to avoid circular dependencies.
 */
export class ValueMapJsonConverter extends JsonConverter<ValueMap<any, any>> {
    /** @inheritdoc */
    get type(): Constructor<ValueMap<any, any>> {
        return ValueMap;
    }

    /** @inheritdoc */
    read(_value: any): ValueMap<any, any> {
        const valueMap = new ValueMap<any, any>();
        // Deserialization is handled separately by JsonSerializer based on field metadata
        return valueMap;
    }

    /** @inheritdoc */
    write(_value: any): any {
        // This method is not used directly - serialization is handled by convertTypesOnInstance
        // in JsonSerializer to properly handle nested type conversions
        return null;
    }
}
