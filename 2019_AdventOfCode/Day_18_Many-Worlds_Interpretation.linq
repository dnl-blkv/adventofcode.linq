<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<string> input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const char CellEntrance = '@';
private const char CellWall = '#';
private const int NoKeys = 0;

private static readonly IReadOnlyList<Point> Directions =
[
    new Point(I:  0, J:  1),
    new Point(I:  1, J:  0),
    new Point(I:  0, J: -1),
    new Point(I: -1, J:  0)
];

private static long Solve1(IReadOnlyList<string> input) => Solve(input);

private static long Solve2(IReadOnlyList<string> input)
{
    string[] cave = input.ToArray();
    Point[] initialPositions = FindEntrances(cave).ToArray();
    
    if (initialPositions.Length is 1)
    {
        (int i, int j) = initialPositions.Single();
        SplitCaveLine(i: i - 1, j: j, splitWith: "@#@");
        SplitCaveLine(i: i, j: j, splitWith: "###");
        SplitCaveLine(i: i + 1, j: j, splitWith: "@#@");
    }
    
    return Solve(cave);
    
    void SplitCaveLine(int i, int j, string splitWith) =>
        cave[i] = $"{cave[i][..(j - 1)]}{splitWith}{cave[i][(j + 2)..]}";
}

private static long Solve(IReadOnlyList<string> cave)
{
    Point[] initialPositions = FindEntrances(cave).ToArray();
    IReadOnlyList<int> keysPerEntrance = initialPositions.Select(p => FindKeys(cave, p)).ToList();
    int allKeys = keysPerEntrance.Aggregate((t, n) => t | n);
    
    (Point[] Positions, int Keys) initialState =
        (Positions: initialPositions, Keys: NoKeys);
    IReadOnlyList<(Point[] Positions, int Keys)> currentStates = [initialState];
    HashSet<(Point Position, int Keys)> visited =
        initialState.Positions.Select(p => (Position: p, Keys: initialState.Keys)).ToHashSet();
        
    int steps = 0;
    
    while (true)
    {
        List<(Point[] Positions, int Keys)> nextStates = [];
    
        foreach ((Point[] currentPositions, int currentKeys) in currentStates)
        {
            for (int i = 0; i < currentPositions.Length; i++)
            {
                if ((keysPerEntrance[i] & ~currentKeys) is NoKeys)
                {
                    continue;
                }
            
                Point currentPosition = currentPositions[i];
            
                foreach (Point direction in Directions)
                {
                    Point nextPosition = currentPosition + direction;
                    char nextCell = cave[nextPosition.I][nextPosition.J];
                    
                    if (nextCell is CellWall
                        || IsDoor(nextCell) && !HasKey(keys: currentKeys, door: nextCell))
                    {
                        continue;
                    }
                    
                    int nextKeys = TryCollectKey(keys: currentKeys, newKey: nextCell);
                    
                    if (!visited.Add((Position: nextPosition, Keys: nextKeys)))
                    {
                        continue;
                    }
                    
                    if (nextKeys == allKeys)
                    {
                        return steps + 1;
                    }
                    
                    nextStates.Add((
                        Positions: GetNextPositions(currentPositions, i, nextPosition),
                        Keys: nextKeys));
                }
            }
        }
        
        currentStates = nextStates;
        steps++;
    }
}

private static IEnumerable<Point> FindEntrances(IReadOnlyList<string> cave)
{
    for (int i = 0; i < cave.Count; i++)
    {
        for (int j = 0; j < cave[i].Length; j++)
        {
            if (cave[i][j] is not CellEntrance)
            {
                continue;
            }
            
            yield return new Point(I: i, J: j);
        }
    }
}

private static int FindKeys(IReadOnlyList<string> cave, Point entrance)
{
    HashSet<Point> visited = [entrance];
    IReadOnlyList<Point> currentPositions = [entrance];
    int keys = NoKeys;
    
    while (currentPositions.Count > 0)
    {
        List<Point> nextPositions = [];
    
        foreach (Point currentPosition in currentPositions)
        {
            foreach (Point nextPosition in Directions.Select(d => currentPosition + d))
            {
                char c = cave[nextPosition.I][nextPosition.J];
            
                if (c is CellWall || !visited.Add(nextPosition))
                {
                    continue;
                }
                
                nextPositions.Add(nextPosition);
                keys = TryCollectKey(keys, newKey: c);
            }
        }
        
        currentPositions = nextPositions;
    }
    
    return keys;
}

private static int TryCollectKey(int keys, char newKey) =>
    IsKey(newKey)
        ? keys | GetKeyMask(newKey)
        : keys;
    
private static bool HasKey(int keys, char door) =>
    (keys & GetKeyMask(char.ToLower(door))) is not NoKeys;
    
private static int GetKeyMask(char key) =>
    1 << (key - 'a');
    
private static bool IsKey(char c) =>
    c is (>= 'a' and <= 'z');
    
private static bool IsDoor(char c) =>
    c is (>= 'A' and <= 'Z');
    
private static Point[] GetNextPositions(Point[] currentPositions, int newPositionIndex, Point newPosition)
{
    Point[] nextPositions = new Point[currentPositions.Length];
    Array.Copy(currentPositions, nextPositions, currentPositions.Length);
    nextPositions[newPositionIndex] = newPosition;
                    
    return nextPositions;   
}

private static IReadOnlyList<string> ParseInput(IEnumerable<string> input) =>
    input.ToArray();

private readonly record struct Point(int I, int J)
{
    public static Point operator +(Point a, Point b) =>
        new Point(I: a.I + b.I, J: a.J + b.J);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
