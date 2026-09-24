// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConceptAs } from '../../ConceptAs';
import { field } from '../../fieldDecorator';
import { Guid } from '../../Guid';

export class Identifier extends ConceptAs<Guid> {
    static readonly valueType = Guid;
}

export class Label extends ConceptAs<string> {}
export class Quantity extends ConceptAs<number> {}
export class Enabled extends ConceptAs<boolean> {}

export class Child {
    @field(Identifier)
    identifier!: Identifier;
}

export class Document {
    @field(Array, { genericArguments: [String] })
    names!: string[];

    @field(Identifier)
    identifier!: Identifier;

    @field(Label)
    label!: Label;

    @field(Quantity)
    quantity!: Quantity;

    @field(Enabled)
    enabled!: Enabled;

    @field(Child)
    child!: Child;

    @field(Array, { genericArguments: [Identifier] })
    identifiers!: Identifier[];

    @field(Array, { genericArguments: [Child] })
    children!: Child[];

    @field(Label, true)
    legacyLabels!: Label[];
}

export class a_model_with_field_metadata {
    readonly identifier = 'f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b';
    readonly payload = {
        names: ['one', 'two'],
        identifier: this.identifier,
        label: '',
        quantity: 0,
        enabled: false,
        child: { identifier: this.identifier },
        identifiers: [this.identifier],
        children: [{ identifier: this.identifier }],
        legacyLabels: ['older']
    };
}
