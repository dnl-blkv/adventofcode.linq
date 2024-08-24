<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyDictionary<CityPair, int> input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static int Solve1(IReadOnlyDictionary<CityPair, int> input) => Solve(
    input,
    intermediateResultComparer: (dist1, dist2) => dist1 <= dist2,
    finalResultSelector: (distances => distances.Min(dist => dist)));

private static int Solve2(IReadOnlyDictionary<CityPair, int> input) => Solve(
    input,
    intermediateResultComparer: (dist1, dist2) => dist1 >= dist2,
    finalResultSelector: (distances => distances.Max(dist => dist)));

private static int Solve(
    IReadOnlyDictionary<CityPair, int> distances,
    Func<int, int, bool> intermediateResultComparer,
    Func<IEnumerable<int>, int> finalResultSelector)
{
    ImmutableHashSet<string> allCities = ImmutableHashSet.Create<string>(GetAllCities(distances).ToArray());
    
    List<(SantaPath sp, int dist)> currentLevel =
        allCities.Select(SantaPath.CreateInitial).Select(sp => (sp, dist: 0)).ToList();
    var minDistances = currentLevel.ToDictionary(t => t.sp, t => t.dist);
    
    while (true)
    {
        var nextLevel = new List<(SantaPath sp, int dist)>();
        
        foreach ((SantaPath sp, int dist) in currentLevel)
        {
            foreach (string nextLocation in allCities.Except(sp.GetVisited()))
            {
                SantaPath nextSp = sp.CreateNext(nextLocation);
                int nextDist = dist + distances[(sp.GetCurrentLocation(), nextLocation)];
                
                if (minDistances.TryGetValue(nextSp, out int existingDistance)
                    && intermediateResultComparer.Invoke(existingDistance, nextDist))
                {
                    continue;
                }
                
                minDistances[nextSp] = nextDist;
                nextLevel.Add((sp: nextSp, dist: nextDist));
            }
        }
        
        if (nextLevel.Count == 0)
        {
            break;
        }
        
        currentLevel = nextLevel;
    }
    
    return finalResultSelector.Invoke(currentLevel.Where(t => t.sp.GetVisited().Count == 8).Select(t => t.dist));
}

private static IEnumerable<string> GetAllCities(IReadOnlyDictionary<CityPair, int> distances) =>
    distances.Keys.SelectMany(cityPair => cityPair.Cities).Distinct();

private static IReadOnlyDictionary<CityPair, int> ParseInput(IEnumerable<string> input)
{
    var parsed = new Dictionary<CityPair, int>();
    
    foreach (string line in input)
    {
        string[] lineParts = line.Split(" = ");
        int distance = int.Parse(lineParts[1].Trim());
        
        string[] cities = lineParts[0].Trim().Split(" to ");
        parsed[(cities[0].Trim(), cities[1].Trim())] = distance; 
    }
    
    return parsed;
}

private class CityPair
{
    private readonly string hashKey;

    private CityPair(string city1, string city2)
    {
        if (city1 == city2)
        {
            throw new InvalidOperationException($"{nameof(city1)} can not be the same as {nameof(city2)}");
        }
        
        this.Cities = ImmutableHashSet.Create<string>(city1, city2);
        this.hashKey = string.Join(',', this.Cities.OrderBy(city => city));
    }
        
    public static implicit operator CityPair((string city1, string city2) tuple) => new CityPair(tuple.city1, tuple.city2);
    
    public ImmutableHashSet<string> Cities { get; }
    
    public override int GetHashCode() => this.hashKey.GetHashCode();
    
    public override bool Equals(object? obj) =>
        !ReferenceEquals(null, obj)
        && (ReferenceEquals(this, obj) || obj is CityPair otherSantaPath && this.hashKey == otherSantaPath.hashKey);
    
    public override string ToString() => this.hashKey;
}

private class SantaPath
{
    private readonly ImmutableHashSet<string> visited;
    private readonly string currentLocation;
    private string hashKey;
    
    private SantaPath(string currentLocation, ImmutableHashSet<string>? visited = null)
    {
        this.visited = visited ?? ImmutableHashSet.Create<string>(currentLocation);
        this.currentLocation = currentLocation;
        this.hashKey = GenerateHashKey(this.currentLocation, this.visited);
    }
    
    public static SantaPath CreateInitial(string currentLocation) => new SantaPath(currentLocation);
    
    public SantaPath CreateNext(string nextLocation)
    {
        if (this.visited.Contains(nextLocation))
        {
            throw new InvalidOperationException("Can't add the same location twice.");
        }
        
        return new SantaPath(nextLocation, visited.Add(nextLocation));
    }
    
    public ImmutableHashSet<string> GetVisited() => this.visited;
    
    public string GetCurrentLocation() => this.currentLocation;
    
    public override int GetHashCode() => this.hashKey.GetHashCode();
    
    public override bool Equals(object? obj) =>
        !ReferenceEquals(null, obj)
        && (ReferenceEquals(this, obj) || obj is SantaPath otherSantaPath && this.hashKey == otherSantaPath.hashKey);
    
    public override string ToString() => this.hashKey;
    
    private static string GenerateHashKey(string currentLocation, ImmutableHashSet<string> visited) =>
        $"{string.Join(';', visited.OrderBy(s => s))}|{currentLocation}";
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}