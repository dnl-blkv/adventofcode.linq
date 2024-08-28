<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<NanoBot> input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static long Solve1(IReadOnlyList<NanoBot> input) =>
    input.MaxBy(b => b.Radius).CountReachable(input);

private static long Solve2(IReadOnlyList<NanoBot> input) =>
    FindCommonOverlapMidPoint(bots: GetLargestOverlapGroup(input)).DistToZero;

/*
    Inequalities used:

    |x - x0| + |y - y0| + |z - z0| <= r0

    x - x0 + y - y0 + z - z0 <= r0
    x - x0 + y - y0 + z0 - z <= r0
    x - x0 + y0 - y + z - z0 <= r0
    x - x0 + y0 - y + z0 - z <= r0
    x0 - x + y - y0 + z - z0 <= r0
    x0 - x + y - y0 + z0 - z <= r0
    x0 - x + y0 - y + z - z0 <= r0
    x0 - x + y0 - y + z0 - z <= r0
*/
private static Point3D FindCommonOverlapMidPoint(IReadOnlyList<NanoBot> bots)
{
    long[] coefficients = Enumerable.Repeat(long.MaxValue, 8).ToArray();
    
    foreach (NanoBot bot in bots)
    {
        ((long x, long y, long z), long r) = bot;
        coefficients[0] = Math.Min(coefficients[0], r + x + y + z);
        coefficients[1] = Math.Min(coefficients[1], r + x + y - z);
        coefficients[2] = Math.Min(coefficients[2], r + x - y + z);
        coefficients[3] = Math.Min(coefficients[3], r + x - y - z);
        coefficients[4] = Math.Min(coefficients[4], r - x + y + z);
        coefficients[5] = Math.Min(coefficients[5], r - x + y - z);
        coefficients[6] = Math.Min(coefficients[6], r - x - y + z);
        coefficients[7] = Math.Min(coefficients[7], r - x - y - z);
    }
    
    long minX = - (coefficients[4] + coefficients[7]) / 2; 
    long maxX = (coefficients[0] + coefficients[3]) / 2;
    long midX = (minX + maxX) / 2;
    
    long minY = - (coefficients[2] + coefficients[7]) / 2; 
    long maxY = (coefficients[0] + coefficients[5]) / 2;
    long midY = (minY + maxY) / 2;
    
    long minZ = - (coefficients[1] + coefficients[7]) / 2; 
    long maxZ = (coefficients[0] + coefficients[6]) / 2;
    long midZ = (minZ + maxZ) / 2;
    
    return new Point3D(X: midX, Y: midY, Z: midZ);
}

private static IReadOnlyList<NanoBot> GetLargestOverlapGroup(IReadOnlyList<NanoBot> input)
{
    HashSet<int>[] overlapGroups = GetRangeOverlaps(input);
    HashSet<int> processed = [];
    
    for (int i = 0; i < overlapGroups.Length; i++)
    {
        if (processed.Contains(i))
        {
            continue;
        }
    
        foreach (int j in overlapGroups[i].ToArray())
        {
            overlapGroups[i].IntersectWith(overlapGroups[j]);
            overlapGroups[j] = overlapGroups[i];
            processed.Add(j);
        }
    }
    
    return overlapGroups.MaxBy(g => g.Count)!.Select(i => input[i]).ToArray();
}

private static HashSet<int>[] GetRangeOverlaps(IReadOnlyList<NanoBot> input) => input
    .Select(b => Enumerable.Range(0, input.Count).Where(j => b.CanReachRangeOf(input[j])).ToHashSet())
    .ToArray();

private static IReadOnlyList<NanoBot> ParseInput(IEnumerable<string> input) =>
    input.Select(NanoBot.Parse).ToArray();

private record struct NanoBot(Point3D Position, long Radius)
{
    public static NanoBot Parse(string line)
    {
        string[] lineParts = line.Split(", ");
        string pointLine = lineParts[0][5..^1];
        string radiusString = lineParts[1][2..];
        
        return new NanoBot(Position: Point3D.Parse(pointLine), Radius: long.Parse(radiusString));
    }
    
    public int CountReachable(IEnumerable<NanoBot> others)
    {
        NanoBot self = this;
        return others.Sum(o => (self.CanReach(o) ? 1 : 0));
    }
    
    public bool CanReachRangeOf(NanoBot other) =>
        this.Position.Dist(other.Position) <= this.Radius + other.Radius;
    
    private bool CanReach(NanoBot other) =>
        this.Position.Dist(other.Position) <= this.Radius;
}

private record struct Point3D(long X, long Y, long Z)
{
    public long DistToZero { get; } = Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);

    public static Point3D Parse(string line)
    {
        long[] coordinates = line.Split(',').Select(long.Parse).ToArray();
        
        return new Point3D(X: coordinates[0], Y: coordinates[1], Z: coordinates[2]);
    }
    
    public long Dist(Point3D other) =>
        Math.Abs(this.X - other.X) + Math.Abs(this.Y - other.Y) + Math.Abs(this.Z - other.Z);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}