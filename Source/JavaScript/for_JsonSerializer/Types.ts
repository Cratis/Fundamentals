// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from '../fieldDecorator';
import { derivedType } from '../derivedTypeDecorator';
import { Guid } from '../Guid';

export class OtherType {
    @field(Number)
    someNumber!: number;

    @field(String)
    someString!: string;

    @field(Date)
    someDate!: Date;

    @field(Guid)
    someGuid!: Guid;

    @field(Date)
    collectionOfDates!: Date[];

    @field(Number)
    collectionOfNumbers!: number[];
}

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface ITargetType { }

@derivedType('ad7593d1-71be-4e26-9026-aedb32fc43d3')
export class FirstDerivative implements ITargetType {
    @field(Number)
    firstDerivativeProperty!: number;
}

@derivedType('a038ca48-360e-46a7-8cb2-882ff21bb623')
export class SecondDerivative implements ITargetType {
    @field(Number)
    secondDerivativeProperty!: number;
}

export class TopLevel {
    @field(Number)
    someNumber!: number;

    @field(String)
    someString!: string;

    @field(Date)
    someDate!: Date;

    @field(Boolean)
    someBoolean!: boolean;

    @field(Guid)
    someGuid!: Guid;

    @field(OtherType)
    otherType!: OtherType;

    @field(OtherType, true)
    collectionOfOtherType!: OtherType[];

    @field(Object, true, [FirstDerivative, SecondDerivative])
    collectionOfDerivedTypes!: ITargetType[];
}
