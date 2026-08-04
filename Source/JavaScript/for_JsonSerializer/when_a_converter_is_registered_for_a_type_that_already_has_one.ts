// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { field } from '../fieldDecorator';
import { Temperature, TemperatureJsonConverter, TemperatureInFahrenheitJsonConverter } from './RegisteredConverterTypes';

class Reading {
    @field(Temperature)
    temperature!: Temperature;
}

describe('when a converter is registered for a type that already has one', () => {
    JsonSerializer.registerConverter(new TemperatureJsonConverter());
    JsonSerializer.registerConverter(new TemperatureInFahrenheitJsonConverter());

    const reading = new Reading();
    reading.temperature = new Temperature(100);

    const serialized = JSON.parse(JsonSerializer.serialize(reading));
    const deserialized = JsonSerializer.deserialize(Reading, '{"temperature":"32F"}');

    it('should serialize through the converter registered last', () => serialized.temperature.should.equal('212F'));
    it('should deserialize through the converter registered last', () => deserialized.temperature.celsius.should.equal(0));
});
