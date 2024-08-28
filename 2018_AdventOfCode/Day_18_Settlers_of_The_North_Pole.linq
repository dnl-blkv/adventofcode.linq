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

private const char Empty = '.';
private const char Tree = '|';
private const char Lumberyard = '#';
private const int StepCount1 = 10;
private const int StepCount2 = 1_000_000_000;

private static readonly IReadOnlyList<Point> Directions =
[
    new Point(I:  0, J:  1),
    new Point(I:  1, J:  1),
    new Point(I:  1, J:  0),
    new Point(I:  1, J: -1),
    new Point(I:  0, J: -1),
    new Point(I: -1, J: -1),
    new Point(I: -1, J:  0),
    new Point(I: -1, J:  1)
];

private static (long Result1, long Result2) Solve(string[] input)
{
    char[][] map = input.Select(l => l.ToCharArray()).ToArray();
    int k = 0;
    long result1 = -1;
    int loopStart2 = -1;
    int loopSize2 = -1;
    var visited = new Dictionary<string, int>
    {
        [ToKey(map)] = 0
    };
    
    do
    {
        char[][] nextMap = input.Select(l => l.Select(_ => Empty).ToArray()).ToArray();
        
        for (int i = 0; i < map.Length; i++)
        {
            for (int j = 0; j < map[i].Length; j++)
            {
                char c = map[i][j];
                nextMap[i][j] = (c, CountNeighborsByType(map, i: i, j: j)) switch
                {
                    (Empty, (_, >= 3, _)) => Tree,
                    (Tree, (_, _, >= 3)) or (Lumberyard, (_, >= 1, >= 1)) => Lumberyard,
                    (Lumberyard, (_, _, _)) => Empty,
                    _ => c
                };
            }
        }
        
        map = nextMap;
        k++;
        
        if (k is StepCount1)
        {
            result1 = GetResourceValue(map);
        }
        
        string newMapKey = ToKey(map);
        
        if (visited.TryGetValue(newMapKey, out int lastK) && loopStart2 < 0)
        {
            loopStart2 = lastK;
            loopSize2 = k - lastK;
            continue;
        }
        
        visited.Add(newMapKey, k);
    } while (result1 < 0 || loopStart2 < 0);
    
    long stepCount2Reduced = loopStart2 + (StepCount2 - loopStart2) % loopSize2;
    long result2 = GetResourceValue(visited.Where(kv => kv.Value == stepCount2Reduced).Single().Key);
    
    return (Result1: result1, Result2: result2);
}
    
private static string ToKey(char[][] map) => string.Join(string.Empty, Flatten(map));
    
private static IEnumerable<char> Flatten(char[][] map) => map.SelectMany(c => c);

private static long GetResourceValue(char[][] map) => GetResourceValue(Flatten(map));

private static long GetResourceValue(IEnumerable<char> cells)
{
    (int _, int t, int l) = CountCellTypes(cells);
    
    return t * l;
}

private static (int E, int T, int L) CountCellTypes(IEnumerable<char> cells)
{
    int e = 0;
    int t = 0;
    int l = 0;

    foreach (Char c in cells)
    {
        e += (c is Empty ? 1 : 0);
        t += (c is Tree ? 1 : 0);
        l += (c is Lumberyard ? 1 : 0);
    }
    
    return (E: e, T: t, L: l);
}

private static (int E, int T, int L) CountNeighborsByType(char[][] map, int i, int j) =>
    CountCellTypes(
        GetNeighbours(map, currentPoint: new Point(I: i, J: j))
            .Select(p => map[p.I][p.J]));

private static IEnumerable<Point> GetNeighbours(char[][] map, Point currentPoint)
{
    foreach (Point direction in Directions)
    {
        Point neighbour = currentPoint + direction;
        
        if (neighbour.I < 0
            || neighbour.I >= map.Length
            || neighbour.J < 0
            || neighbour.J >= map[neighbour.I].Length)
        {
            continue;
        }
        
        yield return neighbour;
    }
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private record struct Point(int I, int J)
{
    public static Point operator +(Point a, Point b) =>
        new Point(I: a.I + b.I, J: a.J + b.J);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
