// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from './Constructor';

/**
 * Identifies a type this package converts, by name rather than by the class object itself.
 *
 * `JsonSerializer` recognized a type by comparing constructors, which holds until a second copy of this
 * package is loaded into the same realm - a nested install, or one install reached as both ESM and
 * CommonJS. Each copy then had its own `Guid`, `ConceptAs` and `ValueMap` class objects and neither
 * recognized the other's, so a `Guid` crossing that boundary was written as an object rather than a
 * string, and a concept as `{"value": ...}` rather than its underlying value. Both silently.
 *
 * A name survives the boundary where a class object does not: the key is a registered symbol, so every
 * copy resolves the same one, and the values are plain strings.
 *
 * The key is deliberately a symbol rather than a plain static: a string static would appear in
 * `Object.keys(Guid)` and could be shadowed by an unrelated member of the same name.
 */
export const typeKey: unique symbol = Symbol.for('@cratis/fundamentals.typeKey');

/** A type carrying the key. Only this package's own convertible types declare one. */
type KeyedType = Constructor & { readonly [typeKey]?: string };

/**
 * Gets the key a type declares itself, ignoring any inherited from a base type.
 *
 * Converter lookup uses this, because a key is inherited: a consumer's `class CustomerId extends
 * ConceptAs<string>` reports `ConceptAs`, and registering a converter under that would take the place
 * of concept handling for every concept in the application.
 * @param {Constructor} type The type to read the key from.
 * @returns {string | undefined} The declared key, or undefined when the type declares none.
 */
export const declaredTypeKey = (type: Constructor | undefined): string | undefined =>
    type && Object.prototype.hasOwnProperty.call(type, typeKey) ? (type as KeyedType)[typeKey] : undefined;

/**
 * Gets the key a type carries, whether it declares the key or inherits it from a base type.
 *
 * This is what makes a concept recognizable across copies: the key is a static, and a static is
 * inherited through the constructor's prototype chain, so every type deriving from `ConceptAs` reports
 * `ConceptAs` without anything having to compare class objects.
 * @param {Constructor} type The type to read the key from.
 * @returns {string | undefined} The key carried, or undefined when the type carries none.
 */
export const typeKeyOf = (type: Constructor | undefined): string | undefined =>
    type ? (type as KeyedType)[typeKey] : undefined;

/** The key every type deriving from `ConceptAs` carries. */
export const conceptAsTypeKey = 'ConceptAs';

/** The key `ValueMap` declares. */
export const valueMapTypeKey = 'ValueMap';
