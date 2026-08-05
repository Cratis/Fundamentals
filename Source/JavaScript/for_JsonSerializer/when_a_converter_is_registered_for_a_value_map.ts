// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { JsonConverter, ValueMapJsonConverter } from '../json';
import { field } from '../fieldDecorator';

/* eslint-disable @typescript-eslint/no-explicit-any */

class CountingValueMapJsonConverter extends JsonConverter<ValueMap<any, any>> {
    get type(): Constructor<ValueMap<any, any>> {
        return ValueMap;
    }

    read(): ValueMap<any, any> {
        return new ValueMap<any, any>().set('read-by', 'converter');
    }

    write(): any {
        return 'written-by-converter';
    }
}

class Holder {
    @field(ValueMap, { genericArguments: [String, String] })
    entries!: ValueMap<string, string>;
}

describe('when a converter is registered for a value map', () => {
    const holder = new Holder();
    holder.entries = new ValueMap<string, string>().set('a', 'b');

    JsonSerializer.registerConverter(new CountingValueMapJsonConverter());
    const serialized = JSON.parse(JsonSerializer.serialize(holder));
    const deserialized = JsonSerializer.deserialize(Holder, '{"entries":{"a":"b"}}');

    // Put the built-in back before any assertion runs. Writing a ValueMap does go through the
    // registered converter, so leaving this one in place would change every later spec that
    // serializes one.
    JsonSerializer.registerConverter(new ValueMapJsonConverter());

    const deserializedKeys = [...deserialized.entries.entries()].map(entry => entry[0]);

    it('should reach the converter on the way out', () => serialized.entries.should.equal('written-by-converter'));
    it('should not reach the converter on the way in', () => deserializedKeys.should.not.contain('read-by'));
    it('should read the field back from the payload instead', () => deserializedKeys.should.deep.equal(['a']));
});
