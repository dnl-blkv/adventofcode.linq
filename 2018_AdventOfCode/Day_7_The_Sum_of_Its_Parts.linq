<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyDictionary<char, IReadOnlyList<char>> input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private const int BaseTimePerStep = 61;
private const int NumWorkers1 = 1;
private const int NumWorkers2 = 5;

private static string Solve1(IReadOnlyDictionary<char, IReadOnlyList<char>> input) =>
    Solve(input, NumWorkers1).Order;

private static long Solve2(IReadOnlyDictionary<char, IReadOnlyList<char>> input) =>
    Solve(input, NumWorkers2).Time;

private static (string Order, int Time) Solve(IReadOnlyDictionary<char, IReadOnlyList<char>> input, int numWorkers)
{
    IEnumerable<KeyValuePair<char, IReadOnlyList<char>>> startSteps =
        input.Keys
            .Concat(input.Values.SelectMany(vs => vs).Distinct())
            .Where(s => !input.ContainsKey(s))
            .Select(k => new KeyValuePair<char, IReadOnlyList<char>>(k, Array.Empty<char>()));
    var dependsOn = new SortedDictionary<char, HashSet<char>>(
        input.Concat(startSteps).ToDictionary(kv => kv.Key, kv => kv.Value.ToHashSet()));
    IReadOnlyDictionary<char, IReadOnlyList<char>> isDependentOn =
        ToReadonlyGroupDict(dependsOn.SelectMany(kv => kv.Value.Select(key => (Key: key, Value: kv.Key))));
        
    int t = 0;
    SortedSet<(int T, char S)> workInProgress = [];
    HashSet<char> visited = [];
    var resultBuilder = new StringBuilder();
    
    do
    {
        if (workInProgress.Count > 0)
        {
            t = workInProgress.Min(q => q.T);
        }
        
        (int T, char S)[] completedSteps = workInProgress.Where(q => q.T == t).ToArray();
        
        foreach ((int T, char S) q in completedSteps)
        {
            (int _, char nextStep) = q;
            
            resultBuilder.Append(nextStep);
            dependsOn.Remove(nextStep);
            workInProgress.Remove(q);
        
            if (!isDependentOn.ContainsKey(nextStep))
            {
                continue;
            }
            
            foreach (char removeFrom in isDependentOn[nextStep])
            {
                dependsOn[removeFrom].Remove(nextStep);
            }
        }
        
        workInProgress.UnionWith(
            dependsOn
                .Where(kv => kv.Value.Count == 0)
                .Select(kv => (T: t + BaseTimePerStep + kv.Key - 'A', S: kv.Key))
                .Where(q => visited.Add(q.S))
                .Take(Math.Max(0, numWorkers - workInProgress.Count)));
    } while (workInProgress.Count > 0);
    
    return (Order: resultBuilder.ToString(), Time: t);
}

private static IReadOnlyDictionary<char, IReadOnlyList<char>> ParseInput(IEnumerable<string> input) =>
    ToReadonlyGroupDict(input.Select(l => (Key: l[^12], Value: l[5])));
        
private static IReadOnlyDictionary<T, IReadOnlyList<T>> ToReadonlyGroupDict<T>(
    IEnumerable<(T Key, T Value)> input)
    where T : notnull =>
    input.GroupBy(kv => kv.Key).ToDictionary(
        g => g.Key,
        g => (IReadOnlyList<T>)g.Select(kv => kv.Value).ToArray());

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}