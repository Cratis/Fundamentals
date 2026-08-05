// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { field } from '../fieldDecorator';

class Coordinates {
    @field(Number)
    latitude!: number;

    @field(Number)
    longitude!: number;
}

class Site {
    @field(Coordinates)
    coordinates!: Coordinates;
}

describe('when no converter is registered for a type', () => {
    const site = new Site();
    site.coordinates = new Coordinates();
    site.coordinates.latitude = 59.94;
    site.coordinates.longitude = 10.72;

    const serialized = JSON.parse(JsonSerializer.serialize(site));
    const deserialized = JsonSerializer.deserialize(Site, '{"coordinates":{"latitude":59.94,"longitude":10.72}}');

    it('should serialize it property by property as before', () => serialized.coordinates.should.deep.equal({ latitude: 59.94, longitude: 10.72 }));
    it('should deserialize it to the declared type', () => deserialized.coordinates.should.be.instanceof(Coordinates));
    it('should deserialize its properties', () => deserialized.coordinates.latitude.should.equal(59.94));
});
