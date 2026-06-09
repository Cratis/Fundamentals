// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { Guid } from '../Guid';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * JSON converter for Guid type.
 */
export class GuidJsonConverter extends JsonConverter<Guid> {
    /** @inheritdoc */
    get type(): Constructor<Guid> {
        return Guid;
    }

    /** @inheritdoc */
    read(value: any): Guid {
        return Guid.parse(value.toString());
    }

    /** @inheritdoc */
    write(value: any): any {
        return value?.toString() ?? '';
    }
}
