// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents a map that compares object keys by value.
 */
export class ValueMap<TKey, TValue> {
    private readonly _entries: Array<[TKey, TValue]> = [];

    /**
     * Adds a new entry or updates an existing entry for the provided key.
     *
     * @param key Key to add or update.
     * @param value Value to associate with the key.
     * @returns The current map instance for chaining.
     */
    set(key: TKey, value: TValue): this {
        const index = this._entries.findIndex(([existingKey]) => this.equals(existingKey, key));
        if (index >= 0) {
            this._entries[index][1] = value;
        } else {
            this._entries.push([key, value]);
        }

        return this;
    }

    /**
     * Gets the value associated with the provided key.
     *
     * @param key Key to look up.
     * @returns The matching value, or undefined when no entry matches.
     */
    get(key: TKey): TValue | undefined {
        return this._entries.find(([existingKey]) => this.equals(existingKey, key))?.[1];
    }

    /**
     * Gets an iterator over all key-value entries in insertion order.
     *
     * @returns Iterator for the map entries.
     */
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