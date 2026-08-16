// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from '../../fieldDecorator';

describe('when invoked for an unsupported standard field without decorator metadata', () => {
    const invoke = () => field(String)(undefined, {
        kind: 'field',
        metadata: undefined,
        name: 'value',
        private: false,
        static: false
    });

    it('should reject the field', () => invoke.should.throw(TypeError, '@field requires standard decorator metadata support'));
});
