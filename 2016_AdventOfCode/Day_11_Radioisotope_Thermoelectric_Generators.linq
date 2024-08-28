<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    State input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int FloorCount = 4;

private static readonly IReadOnlyDictionary<string, string> ElementMap =
    new Dictionary<string, string>
    {
        ["cobalt"] = "Co",
        ["hydrogen"] = "H",
        ["lithium"] = "Li",
        ["polonium"] = "Po",
        ["promethium"] = "Pm",
        ["ruthenium"] = "Ru",
        ["thulium"] = "Tm"
    };

private static long Solve1(State input) => Solve(input);

// This answer seems to always match the formula of: 11 + 12 * (number of complete pairs on the first floor).
// While I did not prove it, the pattern holds for all the known official cases:
// - Test & main input for Pt. 1
// - Main input for Pt.2
//
// It is likely that 11 and 12 are functions of either the number of floors or that of unmatched pairs.
//
// Without the heuristic, the solutiuon completes in around 3.5 minutes on my Xeon-10885M in January 2024.
private static long Solve2(State input) => Solve(input.CloneWithExtraFirstFloorPairs("El", "Li2"));

private static long Solve(State input)
{
    HashSet<State> currentStates = [input];
    var visited = new HashSet<State>(currentStates);
    int result = 0;
    
    while (!currentStates.Any(cs => cs.IsFinal))
    {
        currentStates = currentStates.SelectMany(s => s.GetNextStates()).Where(visited.Add).ToHashSet();
        result++;
    }
    
    return result;
}

private static State ParseInput(IEnumerable<string> input) => State.Parse(input);

private class State(
    IReadOnlyList<HashSet<string>> chips,
    IReadOnlyList<HashSet<string>> generators,
    int currentFloor)
{
    private readonly IReadOnlyList<HashSet<string>> chips = chips;
    private readonly IReadOnlyList<HashSet<string>> generators = generators;
    private readonly int currentFloor = currentFloor;
    
    public string HashString { get; } =
        $"M={GenerateElementsString(chips)}|G={GenerateElementsString(generators)}|F={currentFloor}";
    
    public bool IsFinal =>
        IsFinalElements(this.chips) && IsFinalElements(this.generators);

    public static State Parse(IEnumerable<string> floorStrings)
    {
        var chips = new List<HashSet<string>>(FloorCount);
        var generators = new List<HashSet<string>>(FloorCount);
        
        foreach ((HashSet<string> floorChips, HashSet<string> floorGenerators) in floorStrings.Select(ParseFloor))
        {
            chips.Add(floorChips);
            generators.Add(floorGenerators);
        }
        
        return new State(chips, generators, currentFloor: 0);
    }
        
    public IEnumerable<State> GetNextStates()
    {
        HashSet<string> currentChips = this.chips[currentFloor];
        string[][] chipOnlyMoves = GetOnesOrTwos(currentChips.ToArray()).ToArray();
        
        HashSet<string> currentGenerators = this.generators[currentFloor];
        string[] generatorsWithoutChips = currentGenerators.Except(currentChips).ToArray();
        IEnumerable<string[]> generatorOnlyMovesEnumerable = GetOnesOrTwos(generatorsWithoutChips);
        
        if (currentGenerators.Count is > 0 and <= 2)
        {
            generatorOnlyMovesEnumerable =
                generatorOnlyMovesEnumerable.Append(currentGenerators.ToArray()).Distinct();
        }
        
        string[][] generatorOnlyMoves = generatorOnlyMovesEnumerable.ToArray();
        
        string[] generatorAndChipMoves = currentChips.Intersect(currentGenerators).ToArray();
        
        foreach (int targetFloor in GetAllowedTargetFloors())
        {
            HashSet<string> targetGenerators = this.generators[targetFloor];
        
            foreach (string[] chipOnlyMove in chipOnlyMoves)
            {
                if (targetGenerators.Count > 0 && !chipOnlyMove.All(targetGenerators.Contains))
                {
                    continue;
                }
                
                IReadOnlyList<HashSet<string>> newChips = CloneElements(this.chips);
                newChips[currentFloor].ExceptWith(chipOnlyMove);
                newChips[targetFloor].UnionWith(chipOnlyMove);
                IReadOnlyList<HashSet<string>> newGenerators = CloneElements(this.generators);
                yield return new State(chips: newChips, generators: newGenerators, currentFloor: targetFloor);
            }
            
            HashSet<string> targetChips = this.chips[targetFloor];
            
            foreach (string[] generatorOnlyMove in generatorOnlyMoves)
            {
                if (!targetChips.Except(generatorOnlyMove).All(targetGenerators.Contains))
                {
                    continue;
                }
                
                IReadOnlyList<HashSet<string>> newChips = CloneElements(this.chips);
                IReadOnlyList<HashSet<string>> newGenerators = CloneElements(this.generators);
                newGenerators[currentFloor].ExceptWith(generatorOnlyMove);
                newGenerators[targetFloor].UnionWith(generatorOnlyMove);
                yield return new State(chips: newChips, generators: newGenerators, currentFloor: targetFloor);
            }
            
            foreach (string generatorAndChipMove in generatorAndChipMoves)
            {
                if (!targetChips.All(targetGenerators.Contains))
                {
                    continue;
                }
                
                IReadOnlyList<HashSet<string>> newChips = CloneElements(this.chips);
                newChips[currentFloor].Remove(generatorAndChipMove);
                newChips[targetFloor].Add(generatorAndChipMove);
                IReadOnlyList<HashSet<string>> newGenerators = CloneElements(this.generators);
                newGenerators[currentFloor].Remove(generatorAndChipMove);
                newGenerators[targetFloor].Add(generatorAndChipMove);
                yield return new State(chips: newChips, generators: newGenerators, currentFloor: targetFloor);
            }
        }
    }
    
    public State CloneWithExtraFirstFloorPairs(params string[] elements)
    {
        IReadOnlyList<HashSet<string>> newChips = CloneElements(this.chips);
        IReadOnlyList<HashSet<string>> newGenerators = CloneElements(this.generators);
        
        foreach (string element in elements)
        {
            newChips[0].Add(element);
            newGenerators[0].Add(element);
        }
        
        return new State(chips: newChips, generators: newGenerators, currentFloor: this.currentFloor);
    }
        
    public override bool Equals(object? obj) =>
        obj is State other && this.HashString == other.HashString;

    public override int GetHashCode() =>
        this.HashString.GetHashCode();
    
    private static string GenerateElementsString(IReadOnlyList<HashSet<string>> elements) =>
        string.Join(';', elements.Select((fes, i) => $"{i}:{string.Join(',', fes.OrderBy(e => e))}"));
        
    private static bool IsFinalElements(IReadOnlyList<HashSet<string>> elements) =>
        elements.Take(FloorCount - 1).All(es => es.Count is 0);
        
    private static IEnumerable<T[]> GetOnesOrTwos<T>(IReadOnlyList<T> items)
    {
        foreach (T item in items)
        {
            yield return [item];
        }
        
        for (int i = 0; i < items.Count; i++)
        {
            for (int j = i + 1; j < items.Count; j++)
            {
                yield return [items[i], items[j]];
            }
        }
    }
    
    private static IReadOnlyList<HashSet<string>> CloneElements(IReadOnlyList<HashSet<string>> elements) =>
        elements.Select(es => new HashSet<string>(es)).ToArray();
    
    private static (HashSet<string> Chips, HashSet<string> Generators) ParseFloor(string floorString)
    {
        Dictionary<bool, HashSet<string>> floorItems =
            floorString.Split(" a ")
                .Skip(1)
                .Select(itemLine => itemLine[..itemLine.IndexOf(' ')])
                .GroupBy(itemKey => itemKey.EndsWith("-compatible"))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => ElementMap[string.Concat(e.TakeWhile(c => c != '-'))]).ToHashSet());
                
        return (Chips: GetElements(true), Generators: GetElements(false));

        HashSet<string> GetElements(bool key) =>
            floorItems.TryGetValue(key, out HashSet<string>? elements)
                ? elements
                : new HashSet<string>();
    }
    
    private IEnumerable<int> GetAllowedTargetFloors() =>
        Enumerable.Empty<int>()
            .Append(currentFloor - 1)
            .Append(currentFloor + 1)
            .Where(f => f >= 0 && f < FloorCount);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
