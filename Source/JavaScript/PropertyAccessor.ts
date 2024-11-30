// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * The delegate for representing accessing a property
 */
export type PropertyAccessor<T = object> = (instance: T) => any;
