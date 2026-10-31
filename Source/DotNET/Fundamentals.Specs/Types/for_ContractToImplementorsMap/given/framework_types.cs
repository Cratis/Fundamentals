// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Reflection;

namespace Cratis.Types.for_ContractToImplementorsMap.given;

public class framework_types : an_empty_map
{
    protected Type[] types;
    protected Dictionary<Type, IEnumerable<Type>> previousMappings;

    void Establish()
    {
        types = [.. new[] { typeof(object).Assembly, typeof(Enumerable).Assembly, typeof(System.Xml.XmlDocument).Assembly }
            .SelectMany(_ => _.GetTypes())];
        var mappings = new Dictionary<Type, List<Type>>();
        foreach (var implementor in types.Where(_ => !_.IsInterface && !_.IsAbstract))
        {
            foreach (var contract in implementor.AllBaseAndImplementingTypes())
            {
                if (!mappings.TryGetValue(contract, out var implementors))
                {
                    implementors = [];
                    mappings[contract] = implementors;
                }

                // Preserve the former Contains + Add behavior as the comparison oracle.
                if (!implementors.Contains(implementor)) implementors.Add(implementor);
            }
        }

        previousMappings = mappings.ToDictionary(_ => _.Key, _ => _.Value.AsEnumerable());
    }

    protected void AssertImplementorsMatch()
    {
        foreach (var (contract, implementors) in previousMappings)
        {
            map.GetImplementorsFor(contract).ShouldContainOnly([.. implementors]);
        }
    }
}
