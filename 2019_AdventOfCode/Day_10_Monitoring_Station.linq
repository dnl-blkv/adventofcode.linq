<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
    Solve(input).Dump();
}

private const int DegreesInPI = 180;
private const int LaserShiftDegrees = DegreesInPI * 2 + DegreesInPI / 2;
private const int AsteroidsToEvaporizeCount = 200;

private static readonly List<int> PrimesList = [2, 3];

private static (long Result1, long Result2) Solve(string[] input)
{
    HashSet<Point> asteroids = FindAsteroids(input).ToHashSet();
    (Point station, int maxVisibleAsteroids) =
        asteroids
            .Select(stationCandidate => (
                StationCandidate: stationCandidate,
                VisibleAsteroids: GetAsteroidsOnVectors(stationCandidate).Count(t => t.Asteroids.Any())))
            .MaxBy(t => t.VisibleAsteroids);
    Queue<Point>[] asteroidsOnStationVectors =
        GetAsteroidsOnVectors(station)
            .Select(t => (Vector: t.Vector, Asteroids: new Queue<Point>(t.Asteroids)))
            .OrderBy(t =>
            {
                double vectorAtan = Math.Atan2(y: t.Vector.Y, x: t.Vector.X);
                double vectorAtanDegrees = vectorAtan / Math.PI * DegreesInPI;
            
                return (LaserShiftDegrees + vectorAtanDegrees) % (DegreesInPI * 2);
            })
            .Select(t => t.Asteroids)
            .ToArray();
            
    Point lastAsteroidEvaporized = asteroids.First();

    for (int i = 0; i < AsteroidsToEvaporizeCount; i++)
    {
        Queue<Point> asteroidsOnVector = asteroidsOnStationVectors[i % asteroidsOnStationVectors.Length];
    
        if (asteroidsOnVector.Count is 0)
        {
            continue;
        }
        
        Point currentAsteroid = asteroidsOnVector.Dequeue();
        
        if (i is AsteroidsToEvaporizeCount - 1)
        {
            lastAsteroidEvaporized = currentAsteroid;
            break;
        }
    }
    
    return (
        Result1: maxVisibleAsteroids,
        Result2: lastAsteroidEvaporized.X * 100 + lastAsteroidEvaporized.Y);
    
    IEnumerable<(Point Vector, IEnumerable<Point> Asteroids)> GetAsteroidsOnVectors(Point startFrom) =>
        asteroids
            .Where(a => a != startFrom)
            .Select(targetAsteroid => GetVector(from: startFrom, to: targetAsteroid))
            .Distinct()
            .Select(vector => (
                Vector: vector,
                Asteroids: GetAsteroidsOnVector(startFrom: startFrom, vector: vector)));
    
    IEnumerable<Point> GetAsteroidsOnVector(Point startFrom, Point vector)
    {
        Point currentPosition = startFrom + vector;
    
        while (currentPosition.Y >= 0 && currentPosition.Y < input.Length
            && currentPosition.X >= 0 && currentPosition.X < input[currentPosition.Y].Length)
        {
            if (asteroids.Contains(currentPosition))
            {
                yield return currentPosition;
            }
            
            currentPosition += vector;
        }
    }
}

private static IEnumerable<Point> FindAsteroids(string[] input)
{
    for (int y = 0; y < input.Length; y++)
    {
        for (int x = 0; x < input[y].Length; x++)
        {
            if (input[y][x] is '.')
            {
                continue;
            }
            
            yield return new Point(Y: y, X: x);
        }
    }
}

private static Point GetVector(Point from, Point to)
{
    (int dY, int dX) = to - from;
    
    int dYAbs = Math.Abs(dY);
    int dYSign = (dYAbs is 0 ? 0 : dY / dYAbs);
    int dXAbs = Math.Abs(dX);
    int dXSign = (dXAbs is 0 ? 0 : dX / dXAbs);
    
    if (dYSign * dXSign is 0)
    {
        return new Point(Y: dYSign, X: dXSign);
    }
    
    foreach (int div in GetPrimes().TakeWhile(p => p <= Math.Min(dYAbs, dXAbs)))
    {
        while (dYAbs % div is 0 && dXAbs % div is 0)
        {
            dYAbs /= div;
            dXAbs /= div;
        }
    }
    
    return new Point(Y: dYAbs * dYSign, X: dXAbs * dXSign);
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

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private record struct Point(int Y, int X)
{
    public static Point operator +(Point a, Point b) =>
        new Point(Y: a.Y + b.Y, X: a.X + b.X);
        
    public static Point operator -(Point a, Point b) =>
        new Point(Y: a.Y - b.Y, X: a.X - b.X);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
