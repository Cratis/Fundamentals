// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import './reflection';
import { Constructor } from './Constructor';
import { DerivedType } from './DerivedType';


/* eslint-disable @typescript-eslint/no-explicit-any */

export function derivedType(identifier: string, targetType?: Constructor) {
    return function (target: any) {
        DerivedType.set(target, identifier, targetType);

        // Auto-register with every class in the prototype chain so that JsonSerializer
        // can discover all derived types via DerivedType.getDerivedTypesFor() at runtime,
        // without requiring the parent type to import each subtype (which causes circular deps).
        let proto = Object.getPrototypeOf(target.prototype);
        while (proto && proto.constructor && proto.constructor !== Object) {
            DerivedType.set(target, identifier, proto.constructor);
            proto = Object.getPrototypeOf(proto);
        }
    };
}
