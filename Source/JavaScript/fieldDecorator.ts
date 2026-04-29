// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from './Constructor';
import { Fields } from './Fields';

/* eslint-disable @typescript-eslint/no-explicit-any */

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

    return function (target: any, propertyKey: string) {
        Fields.addFieldToType(target.constructor, propertyKey, targetType, enumerable, actualDerivatives, actualGenericArguments);
    };
}
