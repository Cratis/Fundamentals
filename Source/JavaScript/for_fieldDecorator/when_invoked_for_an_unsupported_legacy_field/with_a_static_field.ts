// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from '../../fieldDecorator';

class Subject { }

describe('when invoked for an unsupported legacy field with a static field', () => {
    const invoke = () => field(String)(Subject, 'value');

    it('should reject the field', () => invoke.should.throw(TypeError, '@field cannot decorate static fields'));
});
