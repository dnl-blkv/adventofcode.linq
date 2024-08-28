<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
    Solve(input).Dump();
}
    
private static readonly Func<(int Y, int X), (int Y, int X)>[] MoveFuncs =
[
    t => (Y: t.Y, t.X + 1),
    t => (Y: t.Y + 1, t.X),
    t => (Y: t.Y, t.X - 1),
    t => (Y: t.Y - 1, t.X),
];

private static readonly IReadOnlyDictionary<char, int> CartDirections =
    new Dictionary<char, int>
    {
        ['>'] = 0,
        ['v'] = 1,
        ['<'] = 2,
        ['^'] = 3
    };

private static (string Result1, string Result2) Solve(string[] input)
{
    SortedDictionary<(int Y, int X), (int D, int T)> carts = FindCarts(input);
    string? result1 = null;
        
    while (carts.Count > 1)
    {
        HashSet<(int Y, int X)> crashSites = [];
        var nextCarts = new SortedDictionary<(int Y, int X), (int D, int T)>();
    
        foreach (((int Y, int X) p, (int d, int t)) in carts.ToArray())
        {
            if (crashSites.Contains(p))
            {
                continue;
            }
        
            carts.Remove(p);
            (int Y, int X) nP = MoveFuncs[d].Invoke(p);
            
            if (carts.ContainsKey(nP) || nextCarts.ContainsKey(nP))
            {
                result1 ??= ToString(nP);
                crashSites.Add(nP);
                carts.Remove(nP);
                nextCarts.Remove(nP);
                continue;
            }
            
            nextCarts.Add(
                nP,
                input[nP.Y][nP.X] switch
                {
                    '/' => (3 - d, t),
                    '\\' => ((d / 2) * 2 + 1 - (d % 2), t),
                    '+' => ((d + t + 3) % 4, (t + 1) % 3),
                    _ => (d, t)
                });
        }
        
        carts = nextCarts;
    }
    
    return (Result1: result1!, Result2: ToString(carts.Single().Key));
    
    string ToString((int Y, int X) p) => $"{p.X},{p.Y}";
}

private static SortedDictionary<(int Y, int X), (int D, int T)> FindCarts(string[] map)
{
    var result = new SortedDictionary<(int Y, int X), (int D, int T)>();

    for (int y = 0; y < map.Length; y++)
    {
        for (int x = 0; x < map[y].Length; x++)
        {
            if (!CartDirections.TryGetValue(map[y][x], out int direction))
            {
                continue;
            }
            
            result.Add((Y: y, X: x), (D: direction, T: 0));
        }
    }
    
    return result;
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
