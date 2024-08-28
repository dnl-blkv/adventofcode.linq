<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int[][] input = ParseInput(GetInput());
    Solve(input).Dump();
}

private static (long Result1, long Result2) Solve(int[][] input)
{
    var componentsByPortTypes = new Dictionary<int, HashSet<int>>();
    
    foreach ((int port, int i) in input.SelectMany((c, i) => c.Select(p => (p, i))))
    {
        if (!componentsByPortTypes.ContainsKey(port))
        {
            componentsByPortTypes[port] = new HashSet<int>();
        }
        
        componentsByPortTypes[port].Add(i);
    }
    
    IReadOnlyList<State> currentStates = [new State(OpenEnd: 0, Strength: 0, Used: [])];
    long maxStrength = 0;
    
    while (true)
    {
        List<State> nextStates = [];
        
        foreach (State currentState in currentStates)
        {
            foreach (int nextIndex in componentsByPortTypes[currentState.OpenEnd].Except(currentState.Used))
            {
                int[] nextEnds = input[nextIndex];
                var nextState = new State(
                    OpenEnd: nextEnds[^(Array.IndexOf(nextEnds, currentState.OpenEnd) + 1)],
                    Strength: currentState.Strength + nextEnds.Sum(),
                    Used: currentState.Used.Append(nextIndex).ToHashSet());
                maxStrength = Math.Max(maxStrength, nextState.Strength);
                nextStates.Add(nextState);
            }
        }
        
        if (nextStates.Count == 0)
        {
            return (Result1: maxStrength, Result2: currentStates.Max(s => s.Strength));
        }
        
        currentStates = nextStates;
    }
}

private static int[][] ParseInput(IEnumerable<string> input) =>
    input.Select(l => l.Split('/').Select(int.Parse).ToArray()).ToArray();

private readonly record struct State(int OpenEnd, int Strength, HashSet<int> Used);

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
