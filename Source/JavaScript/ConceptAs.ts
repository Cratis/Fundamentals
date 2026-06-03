// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Expresses a Concept as another type, usually a primitive such as string or number.
 * @template T The type of the underlying value.
 */
export abstract class ConceptAs<T> {
    /**
     * Initializes a new instance of ConceptAs.
     * @param {T} value The value to initialize the concept with.
     */
    constructor(readonly value: T) {}

    /**
     * Returns the primitive value of the concept.
     * @returns {T} The underlying value.
     */
    valueOf(): T {
        return this.value;
    }

    /**
     * Returns the string representation of the concept.
     * @returns {string} String representation of the underlying value.
     */
    toString(): string {
        return String(this.value);
    }
}
