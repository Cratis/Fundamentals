// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a random byte encoding is undefined.
/// </summary>
/// <param name="encoding">The undefined encoding.</param>
public class UnknownValueEncoding(ValueEncoding encoding) : Exception($"Value encoding '{encoding}' is unknown.");
