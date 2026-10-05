// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { readFileSync } from 'node:fs';
import { Script } from 'node:vm';
import { ModuleKind, ScriptTarget, transpileModule } from 'typescript-programmatic-api';
import * as fundamentals from '../../index';
import { ConceptAs } from '../../ConceptAs';
import { Constructor } from '../../Constructor';
import { field } from '../../fieldDecorator';
import { ValueMap } from '../../ValueMap';

class Label extends ConceptAs<string> {}

class ModelWithNullableFields {
    @field(String)
    scalar?: string | null;

    @field(Label)
    rich?: Label | null;

    @field(Object)
    nested?: { label: string } | null;

    @field(Array, { genericArguments: [String] })
    array?: string[] | null;

    @field(String, true)
    enumerable?: string[] | null;

    @field(ValueMap, { genericArguments: [String, String] })
    map?: ValueMap<string, string> | null;
}

export const decoratorModes = ['legacy', 'standard'] as const;
let standardModel: Constructor<ModelWithNullableFields> | undefined;

export function modelWithNullableFieldsFor(mode: typeof decoratorModes[number]): Constructor<ModelWithNullableFields> {
    if (mode === 'legacy') return ModelWithNullableFields;
    if (standardModel) return standardModel;

    const source = readFileSync(new URL('../fixtures/standard_nullable_fields.ts', import.meta.url), 'utf8');
    const emitted = transpileModule(source, {
        compilerOptions: {
            emitDecoratorMetadata: false,
            experimentalDecorators: false,
            module: ModuleKind.None,
            target: ScriptTarget.ES2022
        },
        fileName: 'standard_nullable_fields.ts'
    }).outputText;
    standardModel = new Script(`${emitted}\nNullableModel`).runInNewContext({ Array, nullableFundamentals: fundamentals, Object, String, Symbol }) as Constructor<ModelWithNullableFields>;
    return standardModel;
}
