// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import './reflection';
import { Constructor } from './Constructor';
import { DerivedType } from './DerivedType';
import { StandardClassDecoratorContext } from './StandardClassDecoratorContext';

export function derivedType(identifier: string, targetType?: Constructor) {
    function decorator<Target extends Constructor>(target: Target): void;
    function decorator<Target extends Constructor>(target: Target, context: StandardClassDecoratorContext<Target>): void;
    function decorator(target: Constructor, context?: StandardClassDecoratorContext<Constructor>): void {
        if (typeof target !== 'function') throw new TypeError('@derivedType can only decorate classes');
        if (context && context.kind !== 'class') throw new TypeError('@derivedType can only decorate classes');
        if (context && typeof context.addInitializer !== 'function') throw new TypeError('@derivedType requires a standard class decorator context');

        if (context) {
            context.addInitializer(function (this: Constructor): void {
                register(this);
            });
            return;
        }

        register(target);
    }

    function register(target: Constructor): void {
        DerivedType.set(target, identifier, targetType);

        // Auto-register with every class in the prototype chain so that JsonSerializer
        // can discover all derived types via DerivedType.getDerivedTypesFor() at runtime,
        // without requiring the parent type to import each subtype (which causes circular deps).
        let proto = Object.getPrototypeOf(target.prototype);
        while (proto && proto.constructor && proto.constructor !== Object) {
            DerivedType.set(target, identifier, proto.constructor);
            proto = Object.getPrototypeOf(proto);
        }
    }

    return decorator;
}
