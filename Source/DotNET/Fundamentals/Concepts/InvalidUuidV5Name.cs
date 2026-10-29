// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a UUID v5 name contains an unpaired surrogate.
/// </summary>
public class InvalidUuidV5Name() : Exception("A UUID v5 name must be well-formed UTF-16 text; it contains an unpaired surrogate.");
