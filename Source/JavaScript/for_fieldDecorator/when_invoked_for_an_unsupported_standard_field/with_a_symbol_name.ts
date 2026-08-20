// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from '../../fieldDecorator';

describe('when invoked for an unsupported standard field with a symbol name', () => {
    const invoke = () => field(String)(undefined, {
        kind: 'field',
        metadata: {},
        name: Symbol('value'),
        private: false,
        static: false
    });

    it('should reject the field', () => invoke.should.throw(TypeError, '@field cannot decorate symbol-named fields'));
});
