// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.Roslyn;

/// <summary>
/// Represents the template for spans.
/// </summary>
public class SpanTemplateData
{
    /// <summary>
    /// Gets or sets the method name.
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the signature of the method.
    /// </summary>
    public string MethodSignature { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the span.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind of the span.
    /// </summary>
    public string Kind { get; set; } = "ActivityKind.Internal";

    /// <summary>
    /// Gets or sets the name of the source parameter.
    /// </summary>
    public string SourceParameter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service type.
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags for the span.
    /// </summary>
    public IEnumerable<TagTemplateData> Tags { get; set; } = [];
}
