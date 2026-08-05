// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { TimeOnly } from '../TimeOnly';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * JSON converter for TimeOnly type.
 */
export class TimeOnlyJsonConverter extends JsonConverter<TimeOnly> {
    /** @inheritdoc */
    get type(): Constructor<TimeOnly> {
        return TimeOnly;
    }

    /** @inheritdoc */
    read(value: any): TimeOnly {
        return TimeOnly.parse(value.toString());
    }

    /** @inheritdoc */
    write(value: any): any {
        return value?.toString() ?? '';
    }
}
