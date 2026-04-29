// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * Represents a map that compares object keys by value.
 */
export class ValueMap<TKey, TValue> {
    private readonly _entries: Array<[TKey, TValue]> = [];

    set(key: TKey, value: TValue): this {
        const index = this._entries.findIndex(([existingKey]) => this.equals(existingKey, key));
        if (index >= 0) {
            this._entries[index][1] = value;
        } else {
            this._entries.push([key, value]);
        }

        return this;
    }

    get(key: TKey): TValue | undefined {
        return this._entries.find(([existingKey]) => this.equals(existingKey, key))?.[1];
    }

    entries(): IterableIterator<[TKey, TValue]> {
        return this._entries.values();
    }

    private equals(first: TKey, second: TKey): boolean {
        if (first === second) {
            return true;
        }

        if (first === undefined || first === null || second === undefined || second === null) {
            return false;
        }

        if (typeof first === 'object' && typeof second === 'object') {
            return JSON.stringify(first) === JSON.stringify(second);
        }

        return false;
    }
}