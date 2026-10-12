// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a generation context has no captured time.
/// </summary>
public class MissingGenerationTime() : Exception("A generation context must contain the time the operation was received.");
