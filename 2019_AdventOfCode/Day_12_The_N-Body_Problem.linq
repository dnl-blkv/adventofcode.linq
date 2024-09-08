<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Point3D[] input = ParseInput(GetInput());
    Solve(input).Dump();
}

private delegate long CoordinateSelector(Point3D point);

private const int IterationCount1 = 1_000;

private static readonly List<int> PrimesList = [2, 3];

private static (long Result1, long Result2) Solve(Point3D[] input)
{
    IReadOnlyList<Body> bodies =
        input.Select((p, i) => new Body(Pos: p, Vel: Point3D.Zero, Index: i)).ToArray();
        
    int i = 0;
    long result1 = -1;
    Dictionary<string, int>[] coordinateStatesSeen =
        Point3D.CoordinateSelectors.Select(_ => new Dictionary<string, int>()).ToArray();
    long[] coordinateLoops = Point3D.CoordinateSelectors.Select(_ => -1L).ToArray();
    long result2 = -1;

    while (result1 < 0 || result2 < 0)
    {
        bodies = GetNextBodies(bodies);
        
        foreach ((CoordinateSelector coordinateSelector, int k) in
            Point3D.CoordinateSelectors.Select((cs, k) => (cs, k)))
        {
            string coordinateStateKey = CreateCoordinateStateKey(bodies, coordinateSelector);
            
            if (coordinateStatesSeen[k].TryAdd(coordinateStateKey, i) || coordinateLoops[k] >= 0)
            {
                continue;
            }
            
            int lastI = coordinateStatesSeen[k][coordinateStateKey];
            coordinateLoops[k] = i - lastI;
        }
        
        if (++i is IterationCount1)
        {
            result1 = bodies.Sum(b => b.Pos.DistToZero * b.Vel.DistToZero);
        }
        
        if (coordinateLoops.All(t => t >= 0) && result2 < 0)
        {
            result2 = coordinateLoops.Aggregate(Lcm);
        }
    }
        
    return (Result1: result1, Result2: result2);
}

private static IReadOnlyList<Body> GetNextBodies(IReadOnlyList<Body> bodies)
{
    long[][] velocityDeltas = bodies.Select(_ => new long[Point3D.CoordinateSelectors.Count]).ToArray();
    
    foreach ((CoordinateSelector coordinateSelector, int coordinateIndex) in
        Point3D.CoordinateSelectors.Select((cs, i) => (cs, i)))
    {
        foreach ((long delta, int index) in GetVelocityDeltas(bodies, coordinateSelector))
        {
            velocityDeltas[index][coordinateIndex] = delta;
        }
    }
    
    return bodies
        .Select((b, i) =>
        {
            (Point3D pos, Point3D vel, int index) = b;
            Point3D newVel = vel + Point3D.FromCoordinates(velocityDeltas[i]);
            Point3D newPos = pos + newVel;
            
            return new Body(Pos: newPos, Vel: newVel, Index: index);
        })
        .ToArray();
}

private static IEnumerable<(long Delta, int Index)> GetVelocityDeltas(
    IReadOnlyList<Body> bodies,
    CoordinateSelector coordinateSelector)
{
    int lastGroupEnd = -1;
            
    foreach ((IReadOnlyList<Body> group, int i) in
        bodies.GroupBy(b => coordinateSelector.Invoke(b.Pos)).OrderBy(g => g.Key).Select((g, i) => (g.ToArray(), i)))
    {
        int groupSize = group.Count;
        int groupStart = lastGroupEnd + 1;
    
        foreach ((Point3D pos, Point3D vel, int index) in group)
        {
            yield return (Delta: bodies.Count - groupStart * 2 - groupSize, Index: index);
        }
        
        lastGroupEnd += groupSize;
    }
}

private static string CreateCoordinateStateKey(
    IEnumerable<Body> bodies,
    CoordinateSelector coordinateSelector) =>
    string.Join('|', bodies.Select(b => $"{coordinateSelector.Invoke(b.Pos)}@{coordinateSelector.Invoke(b.Vel)}"));
    
private static long Lcm(long a, long b)
{
    long cA = a;
    long cB = b;
    long cC = 1;

    foreach (int div in GetPrimes().TakeWhile(p => p <= Math.Min(cA, cB)))
    {
        while (cA % div is 0 && cB % div is 0)
        {
            cA /= div;
            cB /= div;
            cC *= div;
        }
    }
    
    return cA * cB * cC;
}

private static IEnumerable<int> GetPrimes()
{
    int c = 0;
    int i = 0;

    while (true)
    {
        if (c < PrimesList.Count)
        {
            yield return i = PrimesList[c++];
        }
        else if (!PrimesList.Any(p => i % p is 0))
        {   
            PrimesList.Add(i);
        }
        
        i += PrimesList[0];
    }
}

private static Point3D[] ParseInput(IEnumerable<string> input) =>
    input.Select(Point3D.Parse).ToArray();

private record struct Body(Point3D Pos, Point3D Vel, int Index);

private record struct Point3D(long X, long Y, long Z)
{
    public static readonly Point3D Zero = new(X: 0, Y: 0, Z: 0);
    
    public static readonly IReadOnlyList<CoordinateSelector> CoordinateSelectors =
    [
        p => p.X,
        p => p.Y,
        p => p.Z
    ];

    public long DistToZero { get; } = Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
    
    public static Point3D operator +(Point3D a, Point3D b) =>
        new Point3D(X: a.X + b.X, Y: a.Y + b.Y, Z: a.Z + b.Z);
    
    public static Point3D Parse(string line) =>
        Point3D.FromCoordinates(
            line[1..^1].Split(", ").Select(lp => long.Parse(lp[(lp.IndexOf('=') + 1)..])).ToArray());
    
    public static Point3D FromCoordinates(IReadOnlyList<long> coordinates) =>
        new Point3D(X: coordinates[0], Y: coordinates[1], Z: coordinates[2]);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
