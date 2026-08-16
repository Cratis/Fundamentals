// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

describe('when loading with existing Symbol metadata', () => {
    const originalMetadataDescriptor = Object.getOwnPropertyDescriptor(Symbol, 'metadata');
    const existingMetadata = Symbol('existing metadata');
    let actualMetadata: symbol;

    beforeAll(async () => {
        Object.defineProperty(Symbol, 'metadata', {
            configurable: true,
            value: existingMetadata
        });

        await import(/* @vite-ignore */ '../reflection?existingSymbolMetadata');
        actualMetadata = Symbol.metadata;
    });

    afterAll(() => {
        Reflect.deleteProperty(Symbol, 'metadata');
        if (originalMetadataDescriptor) Object.defineProperty(Symbol, 'metadata', originalMetadataDescriptor);
    });

    it('should preserve the existing identity', () => actualMetadata.should.equal(existingMetadata));
});
