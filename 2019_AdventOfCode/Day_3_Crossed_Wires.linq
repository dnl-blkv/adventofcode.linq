<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<IReadOnlyList<Move>> input = ParseInput(GetInput());
    Solve(input).Dump();
}

private static IReadOnlyDictionary<char, Point> Directions =
    new Dictionary<char, Point>()
    {
        ['R'] = new Point(I:  0, J:  1),
        ['D'] = new Point(I:  1, J:  0),
        ['L'] = new Point(I:  0, J: -1),
        ['U'] = new Point(I: -1, J:  0),
    };

private static (long Result1, long Result2) Solve(IReadOnlyList<IReadOnlyList<Move>> input)
{
    IReadOnlyDictionary<Point, int> wire0 = GetWirePoints(input[0]);
    IReadOnlyDictionary<Point, int> wire1 = GetWirePoints(input[1]);
    IReadOnlyList<Point> intersections = wire0.Keys.Intersect(wire1.Keys).Where(p => p != Point.Zero).ToArray();
    
    return (
        Result1: intersections.Min(p => p.DistToZero),
        Result2: intersections.Min(p => wire0[p] + wire1[p]));
}

private static IReadOnlyDictionary<Point, int> GetWirePoints(IReadOnlyList<Move> legs)
{
    Dictionary<Point, int> wirePoints = [];
    Point position = Point.Zero;
    int j = 0;
    
    foreach ((char directionCode, int count) in legs)
    {
        Point direction = Directions[directionCode];
    
        for (int s = 0; s < count; s++)
        {
            j++;
            position += direction;
            
            if (wirePoints.ContainsKey(position))
            {
                continue;
            }
            
            wirePoints[position] = j;
        }
    }
    
    return wirePoints;
}

private static IReadOnlyList<IReadOnlyList<Move>> ParseInput(IEnumerable<string> input) =>
    input
        .Select(line =>
            line.Split(',')
                .Select(moveLine =>
                {
                    char direction = moveLine[0];
                    int count = int.Parse(moveLine[1..]);
                    
                    return new Move(Direction: direction, Count: count);
                })
                .ToArray())
        .ToArray();
        
private record struct Move(char Direction, int Count);

private record struct Point(int I, int J)
{
    public static readonly Point Zero = new Point(I: 0, J: 0);
    
    public int DistToZero => Zero.Dist(this);

    public static Point operator +(Point a, Point b) =>
        new Point(I: a.I + b.I, J: a.J + b.J);
        
    public int Dist(Point other) =>
        Math.Abs(this.I - other.I) + Math.Abs(this.J - other.J);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
