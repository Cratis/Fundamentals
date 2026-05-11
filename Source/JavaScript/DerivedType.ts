// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from './Constructor';

export class DerivedType {
    static set(target: Constructor, identifier: string, targetType?: Constructor) {
        Reflect.defineMetadata('derivedType', identifier, target);
        if (targetType) {
            const existing: Constructor[] = Reflect.getOwnMetadata('derivedTypes', targetType) ?? [];
            if (!existing.includes(target)) {
                existing.push(target);
                Reflect.defineMetadata('derivedTypes', existing, targetType);
            }
        }
    }

    static get(target: Constructor): string {
        return Reflect.getOwnMetadata('derivedType', target);
    }

    static getDerivedTypesFor(targetType: Constructor): Constructor[] {
        return Reflect.getOwnMetadata('derivedTypes', targetType) ?? [];
    }
}
