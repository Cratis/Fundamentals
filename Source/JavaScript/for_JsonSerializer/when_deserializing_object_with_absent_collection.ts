// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from '../fieldDecorator';
import { JsonSerializer } from '../JsonSerializer';


class TheType {
    @field(String)
    someString!: string;

    @field(String, true)
    someCollection!: string[];

    @field(String, true)
    someNullCollection!: string[];
}

const json = '{' +
    '    "someString": "forty two",' +
    '    "someNullCollection": null' +
    '}';

describe('when deserializing object with absent collection', () => {
    const result = JsonSerializer.deserialize(TheType, json);

    it('should hold the value that was there', () => result.someString.should.equal('forty two'));

    // The declared type says this is an array. A producer that leaves an empty collection out of the payload
    // would otherwise hand back undefined behind that type, so every reader has to guard it.
    it('should give the absent collection an empty array', () => result.someCollection.should.be.an('array'));
    it('should give the absent collection no items', () => result.someCollection.length.should.equal(0));

    // An explicit null is the producer saying "no collection", which is not the same as an empty one.
    it('should leave an explicit null alone', () => expect(result.someNullCollection).to.be.null);
});
