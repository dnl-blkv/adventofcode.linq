<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Point[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private const int AllowedDistance2 = 10_000;

private static long Solve1(Point[] input)
{
    Dictionary<Point, int> finiteRegionSizes = input.ToDictionary(p => p, _ => 0);
    (int minX, int maxX, int minY, int maxY) = GetScanRegion(input);
    
    for (int x = minX - 1; x <= maxX + 1; x++)
    {
        for (int y = minY - 1; y <= maxY + 1; y++)
        {
            var currentPoint = new Point(X: x, Y: y);
            (int distToClosestInputPoint, Point closestInputPoint) =
                input.Select(p => (D: currentPoint.Dist(p), P: p)).MinBy(t => t.D);
                
            if (InputExcept(closestInputPoint).All(p => p.Dist(closestInputPoint) < p.Dist(currentPoint)))
            {
                finiteRegionSizes.Remove(closestInputPoint);
            }
            
            if (!finiteRegionSizes.ContainsKey(closestInputPoint))
            {
                continue;
            }
            
            if (distToClosestInputPoint < InputExcept(closestInputPoint).Min(currentPoint.Dist))
            {
                finiteRegionSizes[closestInputPoint]++;
            }
        }
    }
    
    return finiteRegionSizes.Values.Max();
        
    IEnumerable<Point> InputExcept(Point p) => input.Where(o => o != p);
}

/***
With some inputs, the target region could spill outside the Scan Region. In
such cases, this method could be used to find a valid starting point within
the region, and the other points can be counted using flood fill.
***/
private static long Solve2(Point[] input)
{
    long result = 0;
    (int minX, int maxX, int minY, int maxY) = GetScanRegion(input);
    
    for (int x = minX; x <= maxX; x++)
    {
        for (int y = minY; y <= maxY; y++)
        {
            var currentPoint = new Point(X: x, Y: y);
            result += (input.Sum(currentPoint.Dist) < AllowedDistance2 ? 1 : 0);
        }
    }
    
    return result;
}

private static (int MinX, int MaxX, int MinY, int MaxY) GetScanRegion(Point[] input) =>
(
    MinX: input.Min(p => p.X),
    MaxX: input.Max(p => p.X),
    MinY: input.Min(p => p.Y),
    MaxY: input.Max(p => p.Y));

private static Point[] ParseInput(IEnumerable<string> input) =>
    input.Select(Point.Parse).ToArray();

private record struct Point(int X, int Y)
{
    public static Point Parse(string pointString)
    {
        int[] coordinates = pointString.Split(", ").Select(int.Parse).ToArray();
        
        return new Point(X: coordinates[0], Y: coordinates[1]);
    }
    
    public int Dist(Point other) =>
        Math.Abs(this.X - other.X) + Math.Abs(this.Y - other.Y);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}