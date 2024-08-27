<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Layer[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static readonly List<long> PrimeList = [2, 3];
private static readonly HashSet<long> PrimeSet = PrimeList.ToHashSet();

private static long Solve1(Layer[] input) =>
    input.Sum(l => l.NoRem == 0 ? l.Depth * l.Range : 0);

private static long Solve2(Layer[] input)
{
    Dictionary<long, HashSet<long>> noRemsByPeriodMinimal =
        input.GroupBy(l => l.Period).ToDictionary(g => g.Key, g => g.Select(l => l.NoRem).ToHashSet());
    IReadOnlyDictionary<long, long> yesRemsByPeriod =
        GetYesRemsByPeriod(noRemsByPeriodMinimal);
    
    foreach (int period in yesRemsByPeriod.Keys)
    {   
        noRemsByPeriodMinimal.Remove(period);
    }
    
    long result = 0;
    
    while (yesRemsByPeriod.Any(kv => result % kv.Key != kv.Value))
    {
        result++;
    }
    
    long stepSize =
        GetPrimeFactors(yesRemsByPeriod.Keys)
            .Aggregate(1L, (r, kv) => r *= (long)Math.Pow(kv.Key, kv.Value));
    
    while (noRemsByPeriodMinimal.Any(kv => kv.Value.Contains(result % kv.Key)))
    {
        result += stepSize;
    }
    
    return result;
}

private static IReadOnlyDictionary<long, long> GetYesRemsByPeriod(
    IReadOnlyDictionary<long, HashSet<long>> noRemsByPeriodMinimal)
{
    var noRemsByPeriodFull = 
        new SortedDictionary<long, HashSet<long>>(
            noRemsByPeriodMinimal.ToDictionary(kv => kv.Key, kv => new HashSet<long>(kv.Value)));
                    
    foreach (int key in noRemsByPeriodFull.Keys)
    {
        int maxDivKey = (int)Math.Ceiling(Math.Sqrt(key));
        
        foreach (int divKey in noRemsByPeriodFull.Keys.Where(k => k <= maxDivKey))
        {
            if (key % divKey != 0)
            {
                continue;
            }
            
            foreach (int divRem in noRemsByPeriodFull[divKey])
            {
                for (int i = divRem; i < key; i += divKey)
                {
                    noRemsByPeriodFull[key].Add(i);
                }
            }
        }
    }
    
    var yesRemsByPeriod = new Dictionary<long, long>();
    
    foreach ((long period, HashSet<long> rems) in noRemsByPeriodFull)
    {
        if (rems.Count != period - 1)
        {
            continue;
        }
        
        for (int j = 0; j < period; j++)
        {
            if (rems.Contains(j))
            {
                continue;
            }
            
            yesRemsByPeriod[period] = j;
            break;
        }
    }
    
    return yesRemsByPeriod;
}

private static IReadOnlyDictionary<long, long> GetPrimeFactors(IEnumerable<long> ns)
{
    var result = new Dictionary<long, long>();
    
    foreach (long n in ns)
    {
        foreach ((long p, long c) in GetPrimeFactors(n))
        {
            if (!result.ContainsKey(p))
            {
                result[p] = 0;
            }
            
            result[p] = Math.Max(result[p], c);
        }
    }
    
    return result;
}

private static IReadOnlyDictionary<long, long> GetPrimeFactors(long n)
{
    var result = new Dictionary<long, long>();
    long rem = n;
    
    foreach (int prime in GetPrimes(minMax: (long)Math.Ceiling(Math.Sqrt(n))))
    {
        if (rem % prime != 0)
        {
            continue;
        }
        
        if (!result.ContainsKey(prime))
        {
            result[prime] = 0;
        }
        
        while (rem % prime == 0)
        {
            result[prime]++;
            rem /= prime;
        }
    }
    
    if (IsPrime(rem))
    {
        result[rem] = 1;
    }
    
    return result;
}

private static IEnumerable<long> GetPrimes(long minMax = 0)
{
    InitPrimes(minMax);
    
    return PrimeList;
}

private static void InitPrimes(long minMax)
{
    for (long i = PrimeList.Last() + 2; i <= minMax; i += 2)
    {
        if (PrimeList.TakeWhile(p => p <= Math.Sqrt(i)).Any(p => i % p == 0))
        {
            continue;
        }
        
        PrimeList.Add(i);
        PrimeSet.Add(i);
    }
}

private static bool IsPrime(long n)
{
    InitPrimes(minMax: n);
    
    return PrimeSet.Contains(n);
}

private static Layer[] ParseInput(IEnumerable<string> input) =>
    input.Select(Layer.Parse).ToArray();

private record struct Layer
{
    public Layer(int depth, int range)
    {
        this.Depth = depth;
        this.Range = range;
        this.Period = (this.Range - 1) * 2;
        this.NoRem = (this.Period - this.Depth % this.Period) % this.Period;
    }

    public long Depth { get; }
    
    public long Range { get; }

    public long Period { get; }
    
    public long NoRem { get; }

    public static Layer Parse(string layerString)
    {
        string[] layerStringParts = layerString.Split(": ");

        return new Layer(
            depth: int.Parse(layerStringParts[0]),
            range: int.Parse(layerStringParts[1]));
    }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}