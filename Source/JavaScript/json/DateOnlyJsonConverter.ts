// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { DateOnly } from '../DateOnly';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * JSON converter for DateOnly type.
 */
export class DateOnlyJsonConverter extends JsonConverter<DateOnly> {
    /** @inheritdoc */
    get type(): Constructor<DateOnly> {
        return DateOnly;
    }

    /** @inheritdoc */
    read(value: any): DateOnly {
        return DateOnly.parse(value.toString());
    }

    /** @inheritdoc */
    write(value: any): any {
        return value?.toString() ?? '';
    }
}
