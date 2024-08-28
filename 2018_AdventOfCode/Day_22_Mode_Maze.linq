<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (int depth, Point target) = ParseInput(GetInput());
	Solve(depth, target).Dump();
}

private enum GearType
{
    Neither,
    Torch,
    ClimbingGear
}

private const long YZeroGeoIndexMultiplier = 16_807;
private const long XZeroGeoIndexMultiplier = 48_271;
private const long ErosionLevelModulo = 20_183;
private const int GearChangeTime = 7;

private const char RockyTerrain = '.';
private const char WetTerrain = '=';
private const char NarrowTerrain = '|';

private static readonly IReadOnlyList<char> TerrainTypes = [RockyTerrain, WetTerrain, NarrowTerrain];

private static readonly IReadOnlyDictionary<char, HashSet<GearType>> AllowedGearTypes =
    new Dictionary<char, HashSet<GearType>>
    {
        [RockyTerrain] = [GearType.ClimbingGear, GearType.Torch],
        [WetTerrain] = [GearType.Neither, GearType.ClimbingGear],
        [NarrowTerrain] = [GearType.Neither, GearType.Torch]
    };
    
private static readonly IReadOnlyList<Point> Directions =
[
    new Point(Y:  0, X:  1),
    new Point(Y:  1, X:  0),
    new Point(Y:  0, X: -1),
    new Point(Y: -1, X:  0)
];

private static (long Result1, long Result2) Solve(int depth, Point target)
{
    int explorationMultiplier = (GearChangeTime + 1) / 2;
    int heightToExplore = (target.Y + 1) * explorationMultiplier;
    int widthToExplore = (target.X + 1) * explorationMultiplier;
    long[][] erosionLevels = Enumerable.Range(0, heightToExplore).Select(_ => new long[widthToExplore]).ToArray();
    char[][] map = erosionLevels.Select(_ => new char[widthToExplore]).ToArray();
    
    int result1 = 0;
    
    for (int y = 0; y < map.Length; y++)
    {
        for (int x = 0; x < map[y].Length; x++)
        {
            erosionLevels[y][x] = GetErosionLevelForPosition(y: y, x: x);
            int terrainTypeIndex = (int)(erosionLevels[y][x] % TerrainTypes.Count);
            map[y][x] = TerrainTypes[terrainTypeIndex];
            
            if (y > target.Y || x > target.X || target.Y == y && target.X == x)
            {
                continue;
            }
            
            result1 += terrainTypeIndex;
        }
    }

    return (Result1: result1, Result2: GetShortestPathToTarget(map, target));
    
    long GetErosionLevelForPosition(int y, int x) =>
        (y, x) switch
        {
            (0, 0) => GetErosionLevel(0),
            (0, _) => GetErosionLevel(YZeroGeoIndexMultiplier * x),
            (_, 0) => GetErosionLevel(XZeroGeoIndexMultiplier * y),
            (int pY, int pX) when pY == target.Y && pX == target.X => GetErosionLevel(0),
            _ => GetErosionLevel(erosionLevels[y - 1][x] * erosionLevels[y][x - 1])
        };
    
    long GetErosionLevel(long geoIndex) =>
        (geoIndex + depth) % ErosionLevelModulo;
}

private static long GetShortestPathToTarget(char[][] map, Point target)
{
    var initialPosition = new Point(Y: 0, X: 0);
    var initialState = new State(GearType: GearType.Torch, Position: initialPosition, WaitTime: 0);
    HashSet<State> currentStates = [initialState];
    HashSet<State> visited = [initialState];
    int time = 0;
    
    while (true)
    {
        currentStates =
            currentStates
                .SelectMany(currentState =>
                    currentState.WaitTime > 0
                        ? Enumerable.Empty<State>().Append(currentState.Wait())
                        : GetAllPossibleMoves(currentState))
                .Where(visited.Add)
                .ToHashSet();
        time++;
        
        if (!currentStates.Any(s => s.Position == target && s.WaitTime is 0))
        {
            continue;
        }
        
        return time;
    }
    
    IEnumerable<State> GetAllPossibleMoves(State currentState) =>
        GetNextPositions(currentState.Position).SelectMany(nextPosition =>
            GetAllowedGearTypes(currentState.Position, nextPosition).Select(nextGearType =>
                new State(
                    GearType: nextGearType,
                    Position: nextPosition,
                    WaitTime: (nextGearType == currentState.GearType ? 0 : GearChangeTime))));
    
    IEnumerable<Point> GetNextPositions(Point currentPosition) =>
        Directions
            .Select(d => currentPosition + d)
            .Where(p => p.Y >= 0 && p.Y < map.Length && p.X >= 0 && p.X < map[p.Y].Length);
    
    IEnumerable<GearType> GetAllowedGearTypes(Point currentPosition, Point nextPosition) =>
        GetAllowedGearTypesForPosition(currentPosition).Intersect(GetAllowedGearTypesForPosition(nextPosition));
    
    IEnumerable<GearType> GetAllowedGearTypesForPosition(Point p) =>
        p == target
            ? Enumerable.Empty<GearType>().Append(GearType.Torch)
            : AllowedGearTypes[map[p.Y][p.X]];
}

private static (int Depth, Point Target) ParseInput(IEnumerable<string> input)
{
    string[] lines = input.ToArray();
    
    return (Depth: int.Parse(GetValueString(lines[0])), Target: Point.Parse(GetValueString(lines[1])));
    
    static string GetValueString(string line) => line[(line.IndexOf(' ') + 1)..];
}

private record struct Point(int Y, int X)
{
    public Point Up => new Point(Y: this.Y - 1, X: this.X);
    
    public Point Left => new Point(Y: this.Y, X: this.X - 1);
    
    public static Point operator +(Point a, Point b) =>
        new Point(Y: a.Y + b.Y, X: a.X + b.X);
    
    public static Point Parse(string pointLine)
    {
        int[] coordinates = pointLine.Split(',').Select(int.Parse).ToArray();
        
        return new Point(Y: coordinates[1], X: coordinates[0]);
    }
    
    public int Dist(Point other) =>
        Math.Abs(this.Y + other.Y) + Math.Abs(this.X + other.X);
}

private record struct State(GearType GearType, Point Position, int WaitTime)
{
    public State Wait() =>
        new State(GearType: this.GearType, Position: this.Position, WaitTime: this.WaitTime - 1);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}