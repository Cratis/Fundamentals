// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { Script } from 'node:vm';
// TypeScript 7 (the native compiler used for `tsc -b` in this workspace) does not ship the
// legacy programmatic compiler API (createProgram, transpileModule, etc). This spec needs
// that API to compile and execute a decorator fixture in-memory, so it imports a nested
// TypeScript 6 dependency declared under a distinct package name instead of `typescript` -
// see Source/JavaScript/package.json and the TS6/TS7 side-by-side guidance at
// https://devblogs.microsoft.com/typescript/announcing-typescript-7-0/#running-side-by-side-with-typescript-6.0
import { createProgram, getPreEmitDiagnostics, ModuleKind, ModuleResolutionKind, ScriptTarget, transpileModule } from 'typescript-programmatic-api';
import * as fundamentals from '../index';

describe('when executing standard TypeScript decorator emit', () => {
    const originalMetadataDescriptor = Object.getOwnPropertyDescriptor(Symbol, 'metadata');
    let emittedJavaScript: string;
    let symbolMetadataWasAbsent: boolean;
    let symbolMetadataWasPolyfilled: boolean;
    let executionResult: {
        Child: fundamentals.Constructor;
        Circle: fundamentals.Constructor;
        Drawing: fundamentals.Constructor;
        Enabled: fundamentals.Constructor;
        Identifier: fundamentals.Constructor;
        Label: fundamentals.Constructor;
        Quantity: fundamentals.Constructor;
        Rectangle: fundamentals.Constructor;
        ReplacedShape: fundamentals.Constructor;
        Shape: fundamentals.Constructor;
        drawing: {
            child: { identifier: { value: fundamentals.Guid } };
            children: object[];
            createdAt: Date;
            enabled: { value: boolean };
            identifier: { value: fundamentals.Guid };
            identifiers: { value: fundamentals.Guid }[];
            label: { value: string };
            legacyLabels: { value: string }[];
            names: string[];
            quantity: { value: number };
            scores: fundamentals.ValueMap<string, number>;
            shapes: object[];
            title: string;
        };
        instanceCountAfterDeserialize: number;
        instanceCountBeforeDeserialize: number;
        serializedDrawing: string;
    };
    let registeredDerivedTypes: fundamentals.Constructor[];
    let legacySemanticDiagnosticCount: number;
    let semanticDiagnosticCount: number;

    beforeAll(async () => {
        Reflect.deleteProperty(Symbol, 'metadata');
        symbolMetadataWasAbsent = !Object.prototype.hasOwnProperty.call(Symbol, 'metadata');

        await import(/* @vite-ignore */ '../reflection?standardTypeScriptEmit');
        symbolMetadataWasPolyfilled = Symbol.metadata === Symbol.for('Symbol.metadata');

        const fixtureUrl = new URL('./fixtures/standard_decorated_types.ts', import.meta.url);
        const fixturePath = fileURLToPath(fixtureUrl);
        const source = readFileSync(fixtureUrl, 'utf8');
        const compilerOptions = {
            lib: ['lib.esnext.d.ts', 'lib.dom.d.ts'],
            module: ModuleKind.ES2022,
            moduleResolution: ModuleResolutionKind.Bundler,
            noEmit: true,
            noImplicitAny: false,
            skipLibCheck: true,
            strict: true,
            target: ScriptTarget.ES2022
        };
        const standardProgram = createProgram({
            options: {
                ...compilerOptions,
                emitDecoratorMetadata: false,
                experimentalDecorators: false,
            },
            rootNames: [fixturePath]
        });
        const legacyProgram = createProgram({
            options: {
                ...compilerOptions,
                emitDecoratorMetadata: true,
                experimentalDecorators: true,
            },
            rootNames: [fixturePath]
        });
        semanticDiagnosticCount = getPreEmitDiagnostics(standardProgram).length;
        legacySemanticDiagnosticCount = getPreEmitDiagnostics(legacyProgram).length;
        emittedJavaScript = transpileModule(source, {
            compilerOptions: {
                emitDecoratorMetadata: false,
                experimentalDecorators: false,
                module: ModuleKind.None,
                target: ScriptTarget.ES2022
            },
            fileName: 'standard_decorated_types.ts',
            reportDiagnostics: true
        }).outputText;

        const script = new Script(`${emittedJavaScript}\n({ Child, Circle, Drawing, Enabled, Identifier, Label, Quantity, Rectangle, ReplacedShape, Shape, drawing, instanceCountAfterDeserialize, instanceCountBeforeDeserialize, serializedDrawing })`);
        executionResult = script.runInNewContext({ Array, Boolean, Date, fundamentals, JSON, Number, Object, String, Symbol }) as typeof executionResult;
        registeredDerivedTypes = fundamentals.DerivedType.getDerivedTypesFor(executionResult.Shape);
    });

    afterAll(() => {
        Reflect.deleteProperty(Symbol, 'metadata');
        if (originalMetadataDescriptor) Object.defineProperty(Symbol, 'metadata', originalMetadataDescriptor);
    });

    it('should begin without runtime Symbol metadata support', () => symbolMetadataWasAbsent.should.be.true);
    it('should install the Symbol metadata polyfill before class evaluation', () => symbolMetadataWasPolyfilled.should.be.true);
    it('should type check the standard decorator signatures', () => semanticDiagnosticCount.should.equal(0));
    it('should type check the legacy decorator signatures', () => legacySemanticDiagnosticCount.should.equal(0));
    it('should execute standard decorator helper emit', () => emittedJavaScript.should.contain('__esDecorate'));
    it('should expose field metadata before constructing an instance', () => executionResult.instanceCountBeforeDeserialize.should.equal(0));
    it('should construct the target only during first-call deserialization', () => executionResult.instanceCountAfterDeserialize.should.equal(1));
    it('should deserialize primitive fields', () => executionResult.drawing.title.should.equal('Standard decorators'));
    it('should read array elements declared through generic arguments', () => executionResult.drawing.names.should.deep.equal(['one', 'two']));
    it('should read a concept underlying Guid as a Guid', () => executionResult.drawing.identifier.value.should.be.instanceOf(fundamentals.Guid));
    it('should wrap an empty string in its concept', () => {
        (executionResult.drawing.label instanceof executionResult.Label).should.be.true;
        executionResult.drawing.label.value.should.equal('');
    });
    it('should wrap zero in its concept', () => {
        (executionResult.drawing.quantity instanceof executionResult.Quantity).should.be.true;
        executionResult.drawing.quantity.value.should.equal(0);
    });
    it('should wrap false in its concept', () => {
        (executionResult.drawing.enabled instanceof executionResult.Enabled).should.be.true;
        executionResult.drawing.enabled.value.should.equal(false);
    });
    it('should read a nested model and its concept', () => {
        (executionResult.drawing.child instanceof executionResult.Child).should.be.true;
        executionResult.drawing.child.identifier.value.should.be.instanceOf(fundamentals.Guid);
    });
    it('should read an array of concepts', () => {
        (executionResult.drawing.identifiers[0] instanceof executionResult.Identifier).should.be.true;
        executionResult.drawing.identifiers[0].value.should.be.instanceOf(fundamentals.Guid);
    });
    it('should read an array of nested models', () => (executionResult.drawing.children[0] instanceof executionResult.Child).should.be.true);
    it('should keep reading legacy enumerable fields', () => executionResult.drawing.legacyLabels[0].value.should.equal('older'));
    it('should serialize generic arrays of concepts as values', () => JSON.parse(executionResult.serializedDrawing).identifiers.should.deep.equal(['f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b']));
    it('should serialize generic arrays of nested models', () => JSON.parse(executionResult.serializedDrawing).children.should.deep.equal([{ identifier: 'f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b' }]));
    it('should serialize falsy concept values', () => {
        const written = JSON.parse(executionResult.serializedDrawing);
        [written.label, written.quantity, written.enabled].should.deep.equal(['', 0, false]);
    });
    it('should deserialize date fields', () => executionResult.drawing.createdAt.should.be.instanceOf(Date));
    it('should deserialize polymorphic array entries', () => (executionResult.drawing.shapes[0] instanceof executionResult.Circle).should.be.true);
    it('should deserialize every registered derivative', () => (executionResult.drawing.shapes[1] instanceof executionResult.Rectangle).should.be.true);
    it('should auto-register standard derived type decorators', () => registeredDerivedTypes.should.deep.equal([executionResult.Circle, executionResult.Rectangle, executionResult.ReplacedShape]));
    it('should register the final class returned by an outer standard decorator', () => fundamentals.DerivedType.get(executionResult.ReplacedShape).should.equal('replacement'));
    it('should deserialize inherited fields', () => (executionResult.drawing.shapes[0] as { label: string }).label.should.equal('First'));
    it('should deserialize derived fields', () => (executionResult.drawing.shapes[0] as { radius: number }).radius.should.equal(12));
    it('should deserialize generic arguments', () => executionResult.drawing.scores.get('first')!.should.equal(42));
});
