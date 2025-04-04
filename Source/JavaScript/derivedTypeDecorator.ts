// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import 'reflect-metadata';
import { DerivedType } from './DerivedType';


/* eslint-disable @typescript-eslint/no-explicit-any */

export function derivedType(identifier: string) {
    return function (target: any) {
        DerivedType.set(target, identifier);
    };
}
