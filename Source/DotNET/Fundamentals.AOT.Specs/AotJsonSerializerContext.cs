// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Fundamentals.AOT.Specs;

[JsonSerializable(typeof(AotColor))]
[JsonSerializable(typeof(AotColor?))]
[JsonSerializable(typeof(AotPermissions))]
internal sealed partial class AotJsonSerializerContext : JsonSerializerContext;
