<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<Unique<int>> input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int FinalValue = 150;

private static int Solve1(IReadOnlyList<Unique<int>> input) => GetFinalStates(input).Count;

private static int Solve2(IReadOnlyList<Unique<int>> input)
{
    HashSet<State> finalStates = GetFinalStates(input);
    int minCount = finalStates.Min(s => s.Count);
    
    return finalStates.Count(s => s.Count == minCount);
}

private static HashSet<State> GetFinalStates(IReadOnlyList<Unique<int>> input)
{
    var finalStates = new HashSet<State>();
    var currentStates = new HashSet<State>
    {
        new State(input.OrderBy(n => n.ToString()).ToList())
    };
    
    while (currentStates.Any())
    {
        var nextStates = new HashSet<State>();
    
        foreach (State state in currentStates)
        {
            if (state.TotalValue == FinalValue)
            {
                finalStates.Add(state);
                continue;
            }
            
            nextStates.UnionWith(state.GetNextStates().Where(ns => ns.TotalValue <= FinalValue));
        }
        
        currentStates = nextStates;
    }
    
    return finalStates;
}

private static IReadOnlyList<Unique<int>> ParseInput(IEnumerable<string> input) =>
    input.Select(line => new Unique<int>(int.Parse(line))).ToArray();

private class Unique<T>
{
    private static int TotalCount = 0;
    
    private readonly int id;

    public Unique(T value)
    {
        this.Value = value;
        this.id = TotalCount++;
    }

    public T Value { get; }
    
    public override string ToString() => $"{this.Value}[{this.id}]";
}

private class State
{
    private readonly List<Unique<int>> remaining;
    private readonly List<Unique<int>> taken;

    public State(List<Unique<int>> remaining, List<Unique<int>>? taken = null)
    {
        taken ??= new List<Unique<int>>();
    
        this.remaining = remaining;
        this.taken = taken;
        this.HashBase = string.Join('_', this.taken.Select(u => u.ToString()).OrderBy(u => u));
    }
    
    public int Count => this.taken.Count;
    
    public int TotalValue => this.taken.Select(u => u.Value).Sum();
    
    public string HashBase { get; }
    
    public IEnumerable<State> GetNextStates()
    {
        for (int i = 0; i < remaining.Count; i++)
        {   
            var newTaken = new List<Unique<int>>(this.taken.Append(remaining[i]));
            var newRemaining = new List<Unique<int>>(this.remaining);
            newRemaining.RemoveAt(i);
            
            yield return new State(newRemaining, newTaken);
        }
    }
    
    public override bool Equals(object? obj) => obj is State state && state.HashBase == this.HashBase;

    public override int GetHashCode() => this.HashBase.GetHashCode();
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
