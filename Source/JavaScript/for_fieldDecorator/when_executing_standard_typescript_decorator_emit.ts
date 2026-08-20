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
        Circle: fundamentals.Constructor;
        Drawing: fundamentals.Constructor;
        Rectangle: fundamentals.Constructor;
        ReplacedShape: fundamentals.Constructor;
        Shape: fundamentals.Constructor;
        drawing: {
            createdAt: Date;
            scores: fundamentals.ValueMap<string, number>;
            shapes: object[];
            title: string;
        };
        instanceCountAfterDeserialize: number;
        instanceCountBeforeDeserialize: number;
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

        const script = new Script(`${emittedJavaScript}\n({ Circle, Drawing, Rectangle, ReplacedShape, Shape, drawing, instanceCountAfterDeserialize, instanceCountBeforeDeserialize })`);
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
    it('should deserialize date fields', () => executionResult.drawing.createdAt.should.be.instanceOf(Date));
    it('should deserialize polymorphic array entries', () => (executionResult.drawing.shapes[0] instanceof executionResult.Circle).should.be.true);
    it('should deserialize every registered derivative', () => (executionResult.drawing.shapes[1] instanceof executionResult.Rectangle).should.be.true);
    it('should auto-register standard derived type decorators', () => registeredDerivedTypes.should.deep.equal([executionResult.Circle, executionResult.Rectangle, executionResult.ReplacedShape]));
    it('should register the final class returned by an outer standard decorator', () => fundamentals.DerivedType.get(executionResult.ReplacedShape).should.equal('replacement'));
    it('should deserialize inherited fields', () => (executionResult.drawing.shapes[0] as { label: string }).label.should.equal('First'));
    it('should deserialize derived fields', () => (executionResult.drawing.shapes[0] as { radius: number }).radius.should.equal(12));
    it('should deserialize generic arguments', () => executionResult.drawing.scores.get('first')!.should.equal(42));
});
