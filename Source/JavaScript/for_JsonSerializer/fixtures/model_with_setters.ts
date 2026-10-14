// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from '../../fieldDecorator';

export class ModelWithSetters {
    private _label: string = 'default';
    private _quantity: number = 1;

    labelAssignments = 0;
    quantityAssignments = 0;

    @field(String)
    sibling: string | null = null;

    @field(String)
    get label(): string {
        return this._label;
    }

    set label(value: string) {
        this.labelAssignments++;
        this._label = `prefix:${value}`;
    }

    @field(Number)
    get quantity(): number {
        return this._quantity;
    }

    set quantity(value: number) {
        this.quantityAssignments++;
        this._quantity = value * 2;
    }
}
