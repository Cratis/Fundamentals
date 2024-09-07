// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter.given;

public class configured_serialization_options : Specification
{
    protected JsonSerializerOptions _options;

    void Establish()
    {
        _options = new()
        {
            Converters = { new EnumConverterFactory() }
        };
    }
}
