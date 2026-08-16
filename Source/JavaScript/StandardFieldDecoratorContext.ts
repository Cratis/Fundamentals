// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents the standard decorator context used for a class field.
 */
export type StandardFieldDecoratorContext = {
    readonly kind: 'field';
    readonly name: string | symbol;
    readonly static: boolean;
    readonly private: boolean;
    readonly metadata: Record<PropertyKey, unknown> | undefined;
};
