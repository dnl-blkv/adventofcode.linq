<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Body input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const string CenterOfMassBodyName = "COM";
private const string YouBodyName = "YOU";
private const string SantaBodyName = "SAN";

private static long Solve1(Body input)
{
    Dictionary<string, int> childrenCounts = [];
    _ = CountChildren(input);

    return childrenCounts.Values.Sum();
    
    int CountChildren(Body body)
    {
        int result = body.Children.Sum(c => CountChildren(c.Value) + 1);
        childrenCounts[body.Name] = result;
        
        return childrenCounts[body.Name];
    }
}

private static long Solve2(Body input)
{
    Dictionary<string, int> haveYou = [];
    Dictionary<string, int> haveSanta = [];
    HashSet<string> visited = [];
    ScanForYouAndSanta(input);
    
    return
        haveYou.Max(kv => kv.Value)
        + haveSanta.Max(kv => kv.Value)
        - 2 * haveYou.Keys.Intersect(haveSanta.Keys).Max(key => haveYou[key]);
    
    void ScanForYouAndSanta(Body body, int depth = 0)
    {
        string bodyName = body.Name;
    
        if (visited.Contains(bodyName))
        {
            return;
        }
        
        foreach (Body child in body.Children.Values)
        {
            ScanForYouAndSanta(child, depth: depth + 1);
        }
        
        if (body.Children.Any(kv => kv.Key is YouBodyName || haveYou.ContainsKey(kv.Key)))
        {
            haveYou.Add(bodyName, depth);
        }
        
        if (body.Children.Any(kv => kv.Key is SantaBodyName || haveSanta.ContainsKey(kv.Key)))
        {
            haveSanta.Add(bodyName, depth);
        }
        
        visited.Add(bodyName);
    }
}

private static Body ParseInput(IEnumerable<string> input) => Body.ParseBodyTree(input);

private class Body(string name, Dictionary<string, Body> children)
{
    public string Name { get; } = name;
    
    public Dictionary<string, Body> Children { get; } = children;
    
    public static Body ParseBodyTree(IEnumerable<string> bodyMapLines)
    {
        Dictionary<string, HashSet<string>> bodyMap = [];
    
        foreach (string line in bodyMapLines)
        {
            _ = line.Split(')') is [string key, string value]
                ? true
                : throw new ArgumentException($"Can't parse the line: '{line}'");
                
            if (!bodyMap.ContainsKey(key))
            {
                bodyMap[key] = [];
            }
            
            bodyMap[key].Add(value);
        }

        return BuildBodyTree(CenterOfMassBodyName);
        
        Body BuildBodyTree(string bodyName)
        {
            Dictionary<string, Body> children =
                bodyMap.TryGetValue(bodyName, out HashSet<string>? childrenNames)
                    ? childrenNames.Select(cn => (cn, BuildBodyTree(cn))).ToDictionary()
                    : [];
            
            return new Body(bodyName, children);
        }
    }
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
