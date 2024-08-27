<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Particle3D[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static long Solve1(Particle3D[] input) =>
    input.Select((v, i) => (v, i)).MinBy(t => t.v.Acc.DistToZero).i;

private static long Solve2(Particle3D[] input)
{
    HashSet<int> collisions = [];

    for (int i = 0; i < input.Length; i++)
    {
        if (collisions.Contains(i))
        {
            continue;
        }
    
        for (int j = i + 1; j < input.Length; j++)
        {
            if (!input[i].WillCollide(input[j]))
            {
                continue;
            }
            
            collisions.Add(i);
            collisions.Add(j);
        }
    }
    
    return input.Length - collisions.Count;
}

private static Particle3D[] ParseInput(IEnumerable<string> input) =>
    input.Select(ParseParticle).ToArray();
        
private static Particle3D ParseParticle(string particleLine)
{
    Point3D[] vectors = particleLine.Split(", ").Select(ParsePoint).ToArray();
            
    return new Particle3D(Pos: vectors[0], Vel: vectors[1], Acc: vectors[2]);
}
    
private static Point3D ParsePoint(string pointLine)
{
    string coordsLine = pointLine[(pointLine.IndexOf('<') + 1)..^1];
    int[] coords = coordsLine.Split(',').Select(int.Parse).ToArray();
    
    return new Point3D(X: coords[0], Y: coords[1], Z: coords[2]);
}

private record struct Particle3D(Point3D Pos, Point3D Vel, Point3D Acc)
{
    public bool WillCollide(Particle3D other)
    {
        (bool IsAny, decimal[] Times) x = GetIntersections(
            a0: this.Acc.X,
            a1: other.Acc.X,
            v0: this.Vel.X,
            v1: other.Vel.X,
            p0: this.Pos.X,
            p1: other.Pos.X);
        (bool IsAny, decimal[] Times) y = GetIntersections(
            a0: this.Acc.Y,
            a1: other.Acc.Y,
            v0: this.Vel.Y,
            v1: other.Vel.Y,
            p0: this.Pos.Y,
            p1: other.Pos.Y);
        (bool IsAny, decimal[] Times) z = GetIntersections(
            a0: this.Acc.Z,
            a1: other.Acc.Z,
            v0: this.Vel.Z,
            v1: other.Vel.Z,
            p0: this.Pos.Z,
            p1: other.Pos.Z);
        
        (bool IsAny, decimal[] Roots)[] rootsToCheck =
            Enumerable.Empty<(bool IsAny, decimal[] Roots)>()
                .Append(x).Append(y).Append(z)
                .Where(r => !r.IsAny)
                .ToArray();
                
        return rootsToCheck.Length == 0
            || rootsToCheck.Skip(1)
                .Aggregate(
                    (IEnumerable<decimal>)rootsToCheck[0].Roots,
                    (t, n) => t.Intersect(n.Roots))
                .Any(t => t >= 0 && t == (int)t);
    }
    
    private static (bool IsAny, decimal[] Times) GetIntersections(
        decimal a0,
        decimal a1,
        decimal v0,
        decimal v1,
        decimal p0,
        decimal p1)
    {
        decimal a = (a0 - a1) / 2;
        decimal b = v0 - v1 + a;
        decimal c = p0 - p1;
        
        return GetRoots(a: a, b: b, c: c);
    }
    
    private static (bool IsAny, decimal[] Roots) GetRoots(decimal a, decimal b, decimal c)
    {
        if (a == 0)
        {
            return (b == 0 && c == 0)
                ? (IsAny: true, Roots: Array.Empty<decimal>())
                : (IsAny: false, Roots: (b == 0 ? Array.Empty<decimal>() : [-c / b]));
        }
        
        decimal d = b * b - 4 * a * c;
        
        if (d < 0)
        {
            return (IsAny: false, Roots: Array.Empty<decimal>());
        }
        
        decimal dSqrt = (decimal)Math.Sqrt((double)d);
        
        return (
            IsAny: false,
            Roots: ((HashSet<decimal>)[-b + dSqrt, -b - dSqrt]).Select(r => r / (2 * a)).ToArray());
    }
}

private record struct Point3D(int X, int Y, int Z)
{
    public int DistToZero { get; } = Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
    
    public static Point3D operator +(Point3D a, Point3D b) =>
        new Point3D(X: a.X + b.X, Y: a.Y + b.Y, Z: a.Z + b.Z);
    
    public static Point3D operator *(Point3D p, int n) =>
        new Point3D(X: p.X * n, Y: p.Y * n, Z: p.Z * n);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}