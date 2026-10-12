// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when the generation time precedes the Unix epoch.
/// </summary>
/// <param name="time">The unsupported generation time.</param>
public class GenerationTimeBeforeUnixEpoch(DateTimeOffset time) : Exception($"Generation time '{time:O}' must be on or after the Unix epoch.");
