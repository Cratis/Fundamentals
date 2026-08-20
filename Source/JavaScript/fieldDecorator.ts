// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from './Constructor';
import { Field } from './Field';
import { Fields } from './Fields';
import { addFieldToDecoratorMetadata } from './fieldDecoratorMetadata';
import { StandardFieldDecoratorContext } from './StandardFieldDecoratorContext';

type fieldOptions = {
    enumerable?: boolean;
    derivatives?: Constructor[];
    genericArguments?: Constructor[];
};

const isFieldOptions = (value: boolean | fieldOptions | undefined): value is fieldOptions => {
    return typeof value === 'object' && value !== null;
};

export function field(targetType: Constructor, enumerableOrOptions?: boolean | fieldOptions, derivatives?: Constructor[], genericArguments?: Constructor[]) {
    const enumerable = isFieldOptions(enumerableOrOptions) ? (enumerableOrOptions.enumerable || false) : (enumerableOrOptions || false);
    const actualDerivatives = isFieldOptions(enumerableOrOptions) ? (enumerableOrOptions.derivatives || []) : (derivatives || []);
    const actualGenericArguments = isFieldOptions(enumerableOrOptions) ? (enumerableOrOptions.genericArguments || []) : (genericArguments || []);

    function decorator(target: object, propertyKey: string): void;
    function decorator(initialValue: undefined, context: StandardFieldDecoratorContext): void;
    function decorator(targetOrInitialValue: object | undefined, propertyKeyOrContext: string | symbol | StandardFieldDecoratorContext): void {
        if (typeof propertyKeyOrContext === 'object' && propertyKeyOrContext !== null) {
            if (targetOrInitialValue !== undefined) throw new TypeError('@field received an invalid standard field value');
            addStandardField(propertyKeyOrContext);
            return;
        }

        addLegacyField(targetOrInitialValue, propertyKeyOrContext);
    }

    function addStandardField(context: StandardFieldDecoratorContext): void {
        if (context.kind !== 'field') throw new TypeError('@field can only decorate fields');
        if (context.static) throw new TypeError('@field cannot decorate static fields');
        if (context.private) throw new TypeError('@field cannot decorate private fields');
        if (typeof context.name !== 'string') throw new TypeError('@field cannot decorate symbol-named fields');
        if (!context.metadata) throw new TypeError('@field requires standard decorator metadata support');

        addFieldToDecoratorMetadata(
            context.metadata,
            new Field(context.name, targetType, enumerable, actualDerivatives, actualGenericArguments));
    }

    function addLegacyField(target: object | undefined, propertyKey: string | symbol): void {
        if (typeof propertyKey === 'symbol') throw new TypeError('@field cannot decorate symbol-named fields');
        if (typeof propertyKey !== 'string') throw new TypeError('@field received an invalid legacy decorator invocation');
        if (!target || typeof target === 'function') throw new TypeError('@field cannot decorate static fields');

        Fields.addFieldToType(target.constructor as Constructor, propertyKey, targetType, enumerable, actualDerivatives, actualGenericArguments);
    }

    return decorator;
}
