// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { TimeSpan } from '../TimeSpan';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * JSON converter for TimeSpan type.
 */
export class TimeSpanJsonConverter extends JsonConverter<TimeSpan> {
    /** @inheritdoc */
    get type(): Constructor<TimeSpan> {
        return TimeSpan;
    }

    /** @inheritdoc */
    read(value: any): TimeSpan {
        return TimeSpan.parse(value.toString());
    }

    /** @inheritdoc */
    write(value: any): any {
        return value?.toString() ?? '';
    }
}
