// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * JSON converter for Date type.
 */
export class DateJsonConverter extends JsonConverter<Date> {
    /** @inheritdoc */
    get type(): Constructor<Date> {
        return Date;
    }

    /** @inheritdoc */
    read(value: any): Date {
        return new Date(value);
    }

    /** @inheritdoc */
    write(value: any): any {
        return value.toISOString();
    }
}
