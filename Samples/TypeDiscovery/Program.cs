using Cratis.Collections;
using Cratis.Types;

namespace TypeDiscovery;

public static class Program
{
    public static void Main()
    {
        var types = new Types();
        var typesFound = types.FindMultiple<ISomeInterface>();
        typesFound.ForEach(type => Console.WriteLine(type.FullName));
    }
}
