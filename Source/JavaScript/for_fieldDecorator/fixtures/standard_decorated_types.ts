// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

declare const fundamentals: typeof import('../../index');

const { derivedType, field, JsonSerializer, ValueMap } = fundamentals;

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
const drawing = JsonSerializer.deserialize(Drawing, JSON.stringify({
    createdAt: '2026-08-16T09:30:00.000Z',
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

void [Circle, Rectangle, ReplacedShape, instanceCountBeforeDeserialize, drawing, instanceCountAfterDeserialize];
