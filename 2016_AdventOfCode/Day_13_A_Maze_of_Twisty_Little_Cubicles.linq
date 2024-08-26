<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int input = ParseInput(GetInput());
	Solve(input).Dump();
}

private const int StepCount2 = 50;

private static readonly Point TargetPos1 = new Point(I: 39, J: 31);
private static readonly IReadOnlyList<Point> Neighbors =
[
    new Point(I:  0, J:  1),
    new Point(I:  1, J:  0),
    new Point(I:  0, J: -1),
    new Point(I: -1, J:  0)
];

private static (long Result1, long Result2) Solve(int input)
{
    Func<Point, bool> isOpen = CreateIsOpen(input);

    var startingPos = new Point(I: 1, J: 1);
    HashSet<Point> currentStates = [startingPos];
    HashSet<Point> visited = [startingPos];
    long? result1 = null;
    long? result2 = null;
    
    for (int s = 0; result1 is null || result2 is null; s++)
    {
        currentStates =
            currentStates
                .SelectMany(cs => Neighbors.Select(n => cs + n).Where(isOpen).Where(visited.Add))
                .ToHashSet();
                
        if (currentStates.Contains(TargetPos1))
        {
            result1 = s + 1;
        }
        
        if (s == StepCount2 - 1)
        {
            result2 = visited.Count;
        }
    }
    
    return (Result1: result1.Value, Result2: result2.Value);
}

private static Func<Point, bool> CreateIsOpen(int fav)
{
    var field = new Dictionary<Point, bool>();
    
    return pos =>
    {
        if (pos.I < 0 || pos.J < 0)
        {
            return false;
        }
    
        if (field.ContainsKey(pos))
        {
            return field[pos];
        }
    
        (int y, int x) = pos;
        int seed = x * x + 3 * x + 2 * x * y + y + y * y + fav;
        int ones = 0;
        
        while (seed > 0)
        {
            ones += seed % 2;
            seed /= 2;
        }
        
        field[pos] = ones % 2 == 0;
        return field[pos];
    };
}

private static int ParseInput(IEnumerable<string> input) => int.Parse(input.Single());

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