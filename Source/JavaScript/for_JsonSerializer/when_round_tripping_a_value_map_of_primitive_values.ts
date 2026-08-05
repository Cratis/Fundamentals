// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { field } from '../fieldDecorator';

class Settings {
    @field(ValueMap, { genericArguments: [String, String] })
    text!: ValueMap<string, string>;

    @field(ValueMap, { genericArguments: [String, Number] })
    numbers!: ValueMap<string, number>;

    @field(ValueMap, { genericArguments: [String, Boolean] })
    flags!: ValueMap<string, boolean>;
}

describe('when round tripping a value map of primitive values', () => {
    const settings = new Settings();
    settings.text = new ValueMap<string, string>().set('greeting', 'hello');
    settings.numbers = new ValueMap<string, number>().set('retries', 3);
    settings.flags = new ValueMap<string, boolean>().set('enabled', true);

    const written = JsonSerializer.serialize(settings);
    const read = JsonSerializer.deserialize(Settings, written);

    it('should write the values as the primitives they are', () => written.should.equal('{"text":{"greeting":"hello"},"numbers":{"retries":3},"flags":{"enabled":true}}'));
    it('should read a string value back as a string', () => read.text.get('greeting')!.should.equal('hello'));
    it('should read a number value back as a number', () => read.numbers.get('retries')!.should.equal(3));
    it('should read a boolean value back as a boolean', () => read.flags.get('enabled')!.should.equal(true));
    it('should read a string value back as a primitive rather than a String object', () => (typeof read.text.get('greeting')).should.equal('string'));
    it('should read a number value back as a primitive rather than a Number object', () => (typeof read.numbers.get('retries')).should.equal('number'));
    it('should read a boolean value back as a primitive rather than a Boolean object', () => (typeof read.flags.get('enabled')).should.equal('boolean'));
});
