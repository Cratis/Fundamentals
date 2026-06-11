// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Reflect metadata polyfill
 * Provides a minimal implementation of the reflect-metadata API using WeakMap.
 * If native Reflect.getOwnMetadataKeys is already available, this is a no-op.
 */

/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-namespace */

declare global {
    namespace Reflect {
        function defineMetadata(metadataKey: string | symbol, metadataValue: any, target: any): void;
        function getMetadata(metadataKey: string | symbol, target: any): any;
        function getOwnMetadata(metadataKey: string | symbol, target: any): any;
        function hasMetadata(metadataKey: string | symbol, target: any): boolean;
        function hasOwnMetadata(metadataKey: string | symbol, target: any): boolean;
        function getMetadataKeys(target: any): (string | symbol)[];
        function getOwnMetadataKeys(target: any): (string | symbol)[];
        function deleteMetadata(metadataKey: string | symbol, target: any): boolean;
    }
}

if (typeof Reflect.getOwnMetadataKeys !== 'function') {
    const metadataMap = new WeakMap<any, Map<string | symbol, any>>();

    function getMetadataMap(target: any): Map<string | symbol, any> {
        if (!metadataMap.has(target)) {
            metadataMap.set(target, new Map());
        }
        return metadataMap.get(target)!;
    }

    Reflect.defineMetadata = function (metadataKey: string | symbol, metadataValue: any, target: any): void {
        const map = getMetadataMap(target);
        map.set(metadataKey, metadataValue);
    };

    Reflect.getMetadata = function (metadataKey: string | symbol, target: any): any {
        const map = getMetadataMap(target);
        return map.get(metadataKey);
    };

    Reflect.getOwnMetadata = function (metadataKey: string | symbol, target: any): any {
        const map = getMetadataMap(target);
        return map.get(metadataKey);
    };

    Reflect.hasMetadata = function (metadataKey: string | symbol, target: any): boolean {
        const map = getMetadataMap(target);
        return map.has(metadataKey);
    };

    Reflect.hasOwnMetadata = function (metadataKey: string | symbol, target: any): boolean {
        const map = getMetadataMap(target);
        return map.has(metadataKey);
    };

    Reflect.getMetadataKeys = function (target: any): (string | symbol)[] {
        const map = getMetadataMap(target);
        return Array.from(map.keys());
    };

    Reflect.getOwnMetadataKeys = function (target: any): (string | symbol)[] {
        const map = getMetadataMap(target);
        return Array.from(map.keys());
    };

    Reflect.deleteMetadata = function (metadataKey: string | symbol, target: any): boolean {
        const map = getMetadataMap(target);
        return map.delete(metadataKey);
    };
}

// Export a symbol to make this file a module
export {};


