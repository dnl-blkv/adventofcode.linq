<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}
    
private enum VirusState
{
    Clean,
    Weakened,
    Infected,
    Flagged
};

private delegate (VirusState NewState, Direction NewDir) StateMutator(VirusState state, Direction dir);

private const int IterationCount1 = 10_000;
private const int IterationCount2 = 10_000_000;

private static long Solve1(string[] input)
{
    return Solve(input, MutateState, IterationCount1);
    
    static (VirusState NewState, Direction NewDir) MutateState(VirusState state, Direction dir) =>
        state switch
        {
            VirusState.Clean => (NewState: VirusState.Infected, NewDir: dir.TurnLeft()),
            VirusState.Infected => (NewState: VirusState.Clean, NewDir: dir.TurnRight()),
            _ => throw CreateVirusStateNotSupportedException(state)
        };
}

private static long Solve2(string[] input)
{
    return Solve(input, MutateState, IterationCount2);
    
    static (VirusState NewState, Direction NewDir) MutateState(VirusState state, Direction dir) =>
        state switch
        {
            VirusState.Clean => (NewState: VirusState.Weakened, NewDir: dir.TurnLeft()),
            VirusState.Weakened => (NewState: VirusState.Infected, NewDir: dir),
            VirusState.Infected => (NewState: VirusState.Flagged, NewDir: dir.TurnRight()),
            VirusState.Flagged => (NewState: VirusState.Clean, NewDir: dir.Reverse()),
            _ => throw CreateVirusStateNotSupportedException(state)
        };
}

private static long Solve(string[] input, StateMutator stateMutator, int iterationCount)
{
    int height = input.Length;
    int width = input[0].Length;
    
    Dictionary<Point, VirusState> grid = [];
    
    for (int i = 0; i < height; i++)
    {
        for (int j = 0; j < width; j++)
        {
            grid[new Point(I: i, J: j)] =
                (input[i][j] is '#' ? VirusState.Infected : VirusState.Clean);
        }
    }
    
    Point pos = new Point(I: height / 2, J: width / 2);
    Direction dir = Direction.Up;
    int infectionCount = 0;
    
    for (int i = 0; i < iterationCount; i++)
    {
        if (!grid.ContainsKey(pos))
        {
            grid[pos] = VirusState.Clean;
        }
    
        (grid[pos], dir) = stateMutator.Invoke(grid[pos], dir);
        
        if (grid[pos] is VirusState.Infected)
        {
            infectionCount++;
        }
        
        pos = dir.Move(pos);
    }
    
    return infectionCount;
}

private static NotSupportedException CreateVirusStateNotSupportedException(VirusState state) =>
    new NotSupportedException($"VirusState not supported: {state.ToString()}");

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private record Direction(int D)
{
    public static readonly Direction Up = new Direction(D: 3);
    
    private static readonly IReadOnlyList<Point> Directions =
    [
        new Point(I:  0, J:  1),
        new Point(I:  1, J:  0),
        new Point(I:  0, J: -1),
        new Point(I: -1, J:  0)
    ];
    
    public Direction TurnLeft() => Turn(delta: -1);
    
    public Direction TurnRight() => Turn(delta: 1);
    
    public Direction Reverse() => Turn(delta: 2);
    
    public Point Move(Point p) => p + Directions[D];
    
    private Direction Turn(int delta) =>
        new(D: (Directions.Count + this.D + delta % Directions.Count) % Directions.Count);
}
    
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