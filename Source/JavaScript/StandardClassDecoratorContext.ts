// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from './Constructor';

/**
 * Represents the standard decorator context used for a class.
 */
export type StandardClassDecoratorContext<Target extends Constructor> = {
    readonly kind: 'class';
    readonly name: string | undefined;
    readonly metadata: Record<PropertyKey, unknown> | undefined;
    addInitializer(initializer: (this: Target) => void): void;
};
