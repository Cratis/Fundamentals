// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Coordinate } from '../Coordinate';
import { JsonSerializer } from '../JsonSerializer';

describe('when deserializing coordinate with missing properties', () => {
    const json = '{"longitude":10.5}';

    it('should throw an error', () => {
        (() => JsonSerializer.deserialize(Coordinate, json)).should.throw('Cannot deserialize Coordinate: longitude and latitude are required');
    });
});
