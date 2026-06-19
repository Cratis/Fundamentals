// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import './reflection';
import { Constructor } from './Constructor';
import { Field } from './Field';

/**
 * Represents a system working with fields on types.
 */
export class Fields {
    static addFieldToType(target: Constructor, field: string, fieldType: Constructor, enumerable: boolean, derivatives: Constructor[], genericArguments: Constructor[]) {
        let fields: Map<string, Field> = new Map<string, Field>();
        if (Reflect.hasOwnMetadata('fields', target)) {
            fields = Reflect.getOwnMetadata('fields', target);
        }
        fields.set(field, new Field(field, fieldType, enumerable, derivatives, genericArguments));
        Reflect.defineMetadata('fields', fields, target);
    }

    static getFieldsForType(target: Constructor): Field[] {
        // Fields are stored as own-metadata per type, so a derived type only carries the fields it
        // declares itself. Walk the prototype (inheritance) chain and merge every ancestor's fields so
        // that deserializing a derived type also populates the fields inherited from its base types.
        // Process from the base type down to the target so a derived field overrides a base field of the
        // same name.
        const chain: Constructor[] = [];
        let current: Constructor | undefined = target;
        while (current && current !== Function.prototype && current !== Object) {
            chain.push(current);
            current = Object.getPrototypeOf(current) as Constructor | undefined;
        }

        const fieldsByName = new Map<string, Field>();
        for (let index = chain.length - 1; index >= 0; index--) {
            const type = chain[index];
            if (Reflect.hasOwnMetadata('fields', type)) {
                const fieldsMap = Reflect.getOwnMetadata('fields', type) as Map<string, Field>;
                for (const [name, field] of fieldsMap.entries()) {
                    fieldsByName.set(name, field);
                }
            }
        }

        return [...fieldsByName.values()];
    }
}
