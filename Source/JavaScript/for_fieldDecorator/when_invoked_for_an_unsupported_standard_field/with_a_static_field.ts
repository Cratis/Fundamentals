// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from '../../fieldDecorator';

describe('when invoked for an unsupported standard field with a static field', () => {
    const invoke = () => field(String)(undefined, {
        kind: 'field',
        metadata: {},
        name: 'value',
        private: false,
        static: true
    });

    it('should reject the field', () => invoke.should.throw(TypeError, '@field cannot decorate static fields'));
});
