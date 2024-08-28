<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Point4D[] input = ParseInput(GetInput());
	Solve(input).Dump();
}

private const int MaxConstellationEdgeLength = 3;

private static long Solve(Point4D[] input)
{
    HashSet<int>[] constellations = GetNeighbors(input);
    HashSet<HashSet<int>> processed = [];
    
    for (int i = 0; i < constellations.Length; i++)
    {
        foreach (int j in constellations[i].ToArray())
        {
            if (constellations[i] == constellations[j])
            {
                continue;
            }
        
            constellations[i].UnionWith(constellations[j]);
            constellations[j] = constellations[i];
        }
    }
    
    return constellations.Distinct().Count();
}

private static HashSet<int>[] GetNeighbors(IReadOnlyList<Point4D> input) =>
    input.Select((p, i) =>
            Enumerable.Range(i, input.Count - i)
                .Where(j => p.Dist(input[j]) <= MaxConstellationEdgeLength)
                .ToHashSet())
        .ToArray();

private static Point4D[] ParseInput(IEnumerable<string> input) =>
    input.Select(Point4D.Parse).ToArray();

private record struct Point4D(int X, int Y, int Z, int W)
{
    public IEnumerable<int> Coordinates =>
        Enumerable.Empty<int>().Append(this.X).Append(this.Y).Append(this.Z).Append(this.W);

    public static Point4D Parse(string line)
    {
        int[] coordinates = line.Split(',').Select(int.Parse).ToArray();
        
        return new Point4D(X: coordinates[0], Y: coordinates[1], Z: coordinates[2], W: coordinates[3]);
    }
    
    public int Dist(Point4D other) =>
        this.Coordinates.Zip(other.Coordinates).Sum(t => Math.Abs(t.First - t.Second));
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}