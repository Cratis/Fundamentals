// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { JsonConverter } from '../json';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * A domain type the package knows nothing about, standing in for the type a consumer needs to put on
 * the wire in a shape only they can decide.
 */
export class Temperature {
    constructor(readonly celsius: number) {}
}

/** Puts a {@link Temperature} on the wire as the string a consumer chose. */
export class TemperatureJsonConverter extends JsonConverter<Temperature> {
    get type(): Constructor<Temperature> {
        return Temperature;
    }

    read(value: any): Temperature {
        return new Temperature(Number(String(value).replace('C', '')));
    }

    write(value: Temperature): any {
        return `${value.celsius}C`;
    }
}

/** A second, disagreeing converter for the same type, for pinning what a re-registration does. */
export class TemperatureInFahrenheitJsonConverter extends JsonConverter<Temperature> {
    get type(): Constructor<Temperature> {
        return Temperature;
    }

    read(value: any): Temperature {
        return new Temperature((Number(String(value).replace('F', '')) - 32) / 1.8);
    }

    write(value: Temperature): any {
        return `${(value.celsius * 1.8) + 32}F`;
    }
}
