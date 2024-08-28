<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Projectile[] input = ParseInput(GetInput());
    Solve(input).Dump();
}

private const int FontHeight = 10;

private static (string Result1, long Result2) Solve(Projectile[] input)
{
    int bestTime = 0;
    
    for (int i = 0; i < input.Length; i++)
    {
        for (int j = i + 1; j < input.Length; j++)
        {
            bestTime = Math.Max(bestTime, input[i].GetTimeToYDistBelow(input[j], FontHeight));
        }
    }
    
    return (
        Result1: Visualize(input.Select(pj => pj.Position + pj.Velocity * bestTime)),
        Result2: bestTime);
}

private static string Visualize(IEnumerable<Point> points)
{
    HashSet<Point> litUp = points.ToHashSet();
    
    int minX = litUp.Min(p => p.X);
    int maxX = litUp.Max(p => p.X);
    int minY = litUp.Min(p => p.Y);
    int maxY = litUp.Max(p => p.Y);
    
    var resultBuilder = new StringBuilder();
    
    for (int y = minY; y <= maxY; y++)
    {
        for (int x = minX; x <= maxX; x++)
        {
            Point currentPoint = new Point(X: x, Y: y);
            resultBuilder.Append(litUp.Contains(currentPoint) ? '#' : '.');
        }
            
        if (y == maxY)
        {
            continue;
        }
        
        resultBuilder.AppendLine();
    }
    
    return resultBuilder.ToString();
}

private static Projectile[] ParseInput(IEnumerable<string> input) =>
    input.Select(Projectile.Parse).ToArray();

private record struct Projectile(Point Position, Point Velocity)
{
    public static Projectile Parse(string line)
    {
        (int[] pos, int lastPos) = ParseCoordinates(line, startIndex: 0);
        (int[] vel, int _) = ParseCoordinates(line, startIndex: lastPos);
                
        return new Projectile(
            Position: new Point(X: pos[0], Y: pos[1]),
            Velocity: new Point(X: vel[0], Y: vel[1]));
            
        static (int[] Vals, int LastPos) ParseCoordinates(string line, int startIndex)
        {
            int left = line.IndexOf('<', startIndex) + 1;
            int right = line.IndexOf('>', left + 1);
        
            return (
                Vals: line[(left..right)].Split(", ").Select(p => int.Parse(p.Trim())).ToArray(),
                LastPos: right);
        }
    }
    
    public int GetTimeToYDistBelow(Projectile other, int maxYDist)
    {
        int velocity = Math.Abs(this.Velocity.Y - other.Velocity.Y);
        
        if (velocity == 0)
        {
            return 0;
        }
        
        decimal dist = Math.Abs(this.Position.Y - other.Position.Y);
        
        return (int)Math.Ceiling((dist - maxYDist) / velocity);
    }
}

private record struct Point(int X, int Y)
{
    public static Point operator +(Point a, Point b) =>
        new Point(X: a.X + b.X, Y: a.Y + b.Y);
        
    public static Point operator *(Point a, int n) =>
        new Point(X: a.X * n, Y: a.Y * n);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
