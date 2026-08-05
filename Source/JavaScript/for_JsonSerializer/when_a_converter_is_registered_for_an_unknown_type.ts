// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { field } from '../fieldDecorator';
import { Temperature, TemperatureJsonConverter } from './RegisteredConverterTypes';

class Reading {
    @field(Temperature)
    temperature!: Temperature;

    @field(String)
    station!: string;
}

describe('when a converter is registered for an unknown type', () => {
    JsonSerializer.registerConverter(new TemperatureJsonConverter());

    const reading = new Reading();
    reading.temperature = new Temperature(21.5);
    reading.station = 'Blindern';

    const serialized = JSON.parse(JsonSerializer.serialize(reading));
    const deserialized = JsonSerializer.deserialize(Reading, '{"temperature":"18C","station":"Blindern"}');

    it('should serialize through the registered converter', () => serialized.temperature.should.equal('21.5C'));
    it('should leave the other properties alone', () => serialized.station.should.equal('Blindern'));
    it('should deserialize through the registered converter', () => deserialized.temperature.should.be.instanceof(Temperature));
    it('should deserialize to the converted value', () => deserialized.temperature.celsius.should.equal(18));
});
