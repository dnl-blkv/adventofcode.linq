<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (string initialState, IReadOnlyDictionary<string, char> mutations) = ParseInput(GetInput());
    Solve(initialState, mutations).Dump();
}

private const int GenerationCount1 = 20;
private const int MaxExecutedGenerationCount2 = 200;
private const int CycleTriggerDiffRepeatsRequired = 5;
private const long GenerationCount2 = 50_000_000_000;
private const char EmptyCell = '.';

private static (long Result1, long Result2) Solve(string initialState, IReadOnlyDictionary<string, char> mutations)
{
    int paddingSize = Math.Max(GenerationCount1, MaxExecutedGenerationCount2) * 4;
    char[] tunnel = CreatePadding().Concat(initialState).Concat(CreatePadding()).ToArray();
    int n;
    int? result1 = null;
    int lastPlantIndexSum = GetPlantIndexSum();
    int lastDiff = lastPlantIndexSum;
    int diffRepeats = 0;
    
    for (n = 0; n < MaxExecutedGenerationCount2 && diffRepeats < CycleTriggerDiffRepeatsRequired; n++)
    {   
        tunnel = tunnel
            .Select((_, i) =>
            {
                if (i < 2 || tunnel.Length - i <= 2)
                {
                    return EmptyCell;
                }
            
                string regionKey = new string(tunnel[(i - 2)..(i + 3)]);
            
                return mutations.TryGetValue(regionKey, out char newCellValue)
                    ? newCellValue
                    : EmptyCell;
            })
            .ToArray();
        
        int newPlantIndexSum = GetPlantIndexSum();
        
        if (n is GenerationCount1 - 1)
        {
            result1 = newPlantIndexSum;
        }
        
        int newDiff = newPlantIndexSum - lastPlantIndexSum;
        diffRepeats = (newDiff == lastDiff ? diffRepeats + 1 : 0);
        lastDiff = newDiff;
        lastPlantIndexSum = newPlantIndexSum;
    }
    
    long result2 = lastPlantIndexSum + (GenerationCount2 - n) * lastDiff;
    
    return (Result1: result1!.Value, Result2: result2);
    
    IEnumerable<char> CreatePadding() => Enumerable.Repeat(EmptyCell, paddingSize);
    
    int GetPlantIndexSum() => tunnel.Select((c, i) => (c is EmptyCell ? 0 : i - paddingSize)).Sum();
}

private static (string InitialState, IReadOnlyDictionary<string, char> Mutations) ParseInput(
    IEnumerable<string> input)
{
    var initialState = string.Empty;
    Dictionary<string, char> mutations = [];

    foreach ((string line, int i) in input.Select((l, i) => (l, i)))
    {
        if (i == 0)
        {
            initialState = line[15..];
        }
        
        if (i < 2)
        {
            continue;
        }
        
        string[] lineParts = line.Split(" => ");
        mutations[lineParts[0]] = lineParts[1][0];
    }
    
    return (initialState, mutations);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
