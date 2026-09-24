// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

declare const fundamentals: typeof import('../../index');

const { ConceptAs, derivedType, field, Guid, JsonSerializer, ValueMap } = fundamentals;

class Identifier extends ConceptAs<InstanceType<typeof Guid>> {
    static readonly valueType = Guid;
}

class Label extends ConceptAs<string> {}
class Quantity extends ConceptAs<number> {}
class Enabled extends ConceptAs<boolean> {}

class Child {
    @field(Identifier)
    identifier!: Identifier;
}

function replaceClass<Target extends import('../../Constructor').Constructor>(target: Target): Target {
    return class extends target { } as Target;
}

class Shape {
    @field(String)
    label!: string;
}

@derivedType('circle')
class Circle extends Shape {
    @field(Number)
    radius!: number;
}

@derivedType('rectangle')
class Rectangle extends Shape {
    @field(Number)
    width!: number;

    @field(Number)
    height!: number;
}

@replaceClass
@derivedType('replacement')
class ReplacedShape extends Shape { }

class Drawing {
    static instanceCount = 0;

    @field(String)
    title!: string;

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

    @field(Date)
    createdAt!: Date;

    @field(Shape, true)
    shapes!: Shape[];

    @field(ValueMap, { genericArguments: [String, Number] })
    scores!: InstanceType<typeof ValueMap<string, number>>;

    constructor() {
        Drawing.instanceCount++;
    }
}

const instanceCountBeforeDeserialize = Drawing.instanceCount;
const identifier = 'f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b';
const drawing = JsonSerializer.deserialize(Drawing, JSON.stringify({
    child: { identifier },
    children: [{ identifier }],
    createdAt: '2026-08-16T09:30:00.000Z',
    enabled: false,
    identifier,
    identifiers: [identifier],
    label: '',
    legacyLabels: ['older'],
    names: ['one', 'two'],
    quantity: 0,
    scores: {
        first: 42
    },
    shapes: [{
        _derivedTypeId: 'circle',
        label: 'First',
        radius: 12
    }, {
        _derivedTypeId: 'rectangle',
        height: 4,
        label: 'Second',
        width: 8
    }],
    title: 'Standard decorators'
}));
const instanceCountAfterDeserialize = Drawing.instanceCount;
const serializedDrawing = JsonSerializer.serialize(drawing);

void [Child, Circle, Rectangle, ReplacedShape, instanceCountBeforeDeserialize, drawing, instanceCountAfterDeserialize, serializedDrawing];
