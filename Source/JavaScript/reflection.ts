// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Reflect metadata polyfill
 * Provides a minimal implementation of the reflect-metadata API using WeakMap.
 * If native Reflect metadata API is already available, this is a no-op.
 *
 * Limitations:
 * - Only supports object targets (not primitives)
 * - Property-level metadata uses a secondary Map structure keyed by property
 * - Does not support all advanced reflect-metadata features
 */

/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-namespace */

declare global {
    namespace Reflect {
        function defineMetadata(metadataKey: string | symbol, metadataValue: any, target: any, propertyKey?: string | symbol): void;
        function getMetadata(metadataKey: string | symbol, target: any, propertyKey?: string | symbol): any;
        function getOwnMetadata(metadataKey: string | symbol, target: any, propertyKey?: string | symbol): any;
        function hasMetadata(metadataKey: string | symbol, target: any, propertyKey?: string | symbol): boolean;
        function hasOwnMetadata(metadataKey: string | symbol, target: any, propertyKey?: string | symbol): boolean;
        function getMetadataKeys(target: any, propertyKey?: string | symbol): (string | symbol)[];
        function getOwnMetadataKeys(target: any, propertyKey?: string | symbol): (string | symbol)[];
        function deleteMetadata(metadataKey: string | symbol, target: any, propertyKey?: string | symbol): boolean;
        function metadata(metadataKey: string | symbol, metadataValue: any): (target: any, propertyKey?: string | symbol) => void;
    }
}

// Check if native metadata API is available by testing for multiple methods
if (typeof Reflect.defineMetadata !== 'function' || typeof Reflect.getOwnMetadata !== 'function') {
    const metadataMap = new WeakMap<any, Map<string | symbol, any>>();
    const propertyMetadataMap = new WeakMap<any, Map<string | symbol, Map<string | symbol, any>>>();

    function getMetadataStore(target: any, propertyKey?: string | symbol): Map<string | symbol, any> {
        if (propertyKey != null) {
            let propMap = propertyMetadataMap.get(target);
            if (!propMap) {
                propMap = new Map();
                propertyMetadataMap.set(target, propMap);
            }
            if (!propMap.has(propertyKey)) {
                propMap.set(propertyKey, new Map());
            }
            return propMap.get(propertyKey)!;
        } else {
            if (!metadataMap.has(target)) {
                metadataMap.set(target, new Map());
            }
            return metadataMap.get(target)!;
        }
    }

    function getMetadataStoreIfExists(target: any, propertyKey?: string | symbol): Map<string | symbol, any> | undefined {
        if (propertyKey != null) {
            const propMap = propertyMetadataMap.get(target);
            return propMap?.get(propertyKey);
        } else {
            return metadataMap.get(target);
        }
    }

    Reflect.defineMetadata = function (metadataKey: string | symbol, metadataValue: any, target: any, propertyKey?: string | symbol): void {
        const store = getMetadataStore(target, propertyKey);
        store.set(metadataKey, metadataValue);
    };

    Reflect.getMetadata = function (metadataKey: string | symbol, target: any, propertyKey?: string | symbol): any {
        let currentTarget = target;
        while (currentTarget !== null && currentTarget !== undefined) {
            const store = getMetadataStoreIfExists(currentTarget, propertyKey);
            if (store && store.has(metadataKey)) {
                return store.get(metadataKey);
            }
            currentTarget = Object.getPrototypeOf(currentTarget);
        }
        return undefined;
    };

    Reflect.getOwnMetadata = function (metadataKey: string | symbol, target: any, propertyKey?: string | symbol): any {
        const store = getMetadataStoreIfExists(target, propertyKey);
        return store?.get(metadataKey);
    };

    Reflect.hasMetadata = function (metadataKey: string | symbol, target: any, propertyKey?: string | symbol): boolean {
        let currentTarget = target;
        while (currentTarget !== null && currentTarget !== undefined) {
            const store = getMetadataStoreIfExists(currentTarget, propertyKey);
            if (store && store.has(metadataKey)) {
                return true;
            }
            currentTarget = Object.getPrototypeOf(currentTarget);
        }
        return false;
    };

    Reflect.hasOwnMetadata = function (metadataKey: string | symbol, target: any, propertyKey?: string | symbol): boolean {
        const store = getMetadataStoreIfExists(target, propertyKey);
        return store ? store.has(metadataKey) : false;
    };

    Reflect.getMetadataKeys = function (target: any, propertyKey?: string | symbol): (string | symbol)[] {
        const keys = new Set<string | symbol>();
        let currentTarget = target;
        while (currentTarget !== null && currentTarget !== undefined) {
            const store = getMetadataStoreIfExists(currentTarget, propertyKey);
            if (store) {
                for (const key of store.keys()) {
                    keys.add(key);
                }
            }
            currentTarget = Object.getPrototypeOf(currentTarget);
        }
        return Array.from(keys);
    };

    Reflect.getOwnMetadataKeys = function (target: any, propertyKey?: string | symbol): (string | symbol)[] {
        const store = getMetadataStoreIfExists(target, propertyKey);
        return store ? Array.from(store.keys()) : [];
    };

    Reflect.deleteMetadata = function (metadataKey: string | symbol, target: any, propertyKey?: string | symbol): boolean {
        const store = getMetadataStoreIfExists(target, propertyKey);
        return store ? store.delete(metadataKey) : false;
    };

    // Decorator factory used by TypeScript's emitDecoratorMetadata output (the tslib
    // __metadata helper). Without this, 'design:type'/'design:paramtypes'/'design:returntype'
    // metadata is never recorded, which breaks constructor parameter type reflection.
    Reflect.metadata = function (metadataKey: string | symbol, metadataValue: any): (target: any, propertyKey?: string | symbol) => void {
        return function (target: any, propertyKey?: string | symbol): void {
            Reflect.defineMetadata(metadataKey, metadataValue, target, propertyKey);
        };
    };
}

export {};
