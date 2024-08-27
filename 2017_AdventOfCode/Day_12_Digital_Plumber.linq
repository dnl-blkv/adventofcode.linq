<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyDictionary<int, HashSet<int>> input = ParseInput(GetInput());
	Solve(input).Dump();
}

private static (long Result1, long Result2) Solve(IReadOnlyDictionary<int, HashSet<int>> input)
{
    int initialState = 0;
    Dictionary<int, HashSet<int>> groups = [];
    HashSet<int> unvisited = input.Keys.ToHashSet();
    
    while (unvisited.Count > 0)
    {
        groups[initialState] = GetGroup(input, initialState, unvisited);
        unvisited.ExceptWith(groups[initialState]);
        initialState = unvisited.FirstOrDefault();
    }
    
    return (Result1: groups[0].Count, Result2: groups.Count);
}

private static HashSet<int> GetGroup(
    IReadOnlyDictionary<int, HashSet<int>> input,
    int initialState,
    HashSet<int> unvisited)
{
    HashSet<int> currentStates = [initialState];
    HashSet<int> visited = [initialState];
    
    while (currentStates.Count > 0)
    {
        currentStates =
            currentStates.SelectMany(s => input[s]).Where(unvisited.Contains).Where(visited.Add).ToHashSet();
    }
    
    return visited;
}

private static IReadOnlyDictionary<int, HashSet<int>> ParseInput(IEnumerable<string> input) =>
    input
        .Select(l =>
        {
            string[] lineParts = l.Split(" <-> ");
            int from = int.Parse(lineParts[0]);
            HashSet<int> to = lineParts[1].Split(", ").Select(int.Parse).ToHashSet();
            
            return (From: from, To: to);
        })
        .ToDictionary(t => t.From, t => t.To);

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}