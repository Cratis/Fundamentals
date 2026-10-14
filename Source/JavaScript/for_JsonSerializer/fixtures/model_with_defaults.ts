// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from '../../fieldDecorator';
import { ChildWithDefaults } from './child_with_defaults';

export class ModelWithDefaults {
    readonly original = { label: 'original' };
    readonly originalChild = new ChildWithDefaults();

    @field(String)
    nullable: string | null = null;

    @field(String)
    label: string = 'default';

    @field(Number)
    quantity: number = 42;

    @field(Boolean)
    enabled: boolean = true;

    @field(Object)
    mirror = this.original;

    @field(Object)
    self = this;

    @field(String)
    uninitialized?: string;

    @field(Array, { genericArguments: [String] })
    array: string[] | null = ['default'];

    @field(String, true)
    enumerable: string[] | null = ['default'];

    @field(ChildWithDefaults)
    child = this.originalChild;
}
