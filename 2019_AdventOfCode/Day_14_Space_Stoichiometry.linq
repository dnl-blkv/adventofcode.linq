<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyDictionary<string, Blueprint> input = ParseInput(GetInput());
    Solve(input).Dump();
}

private const string BaseInput = "ORE";
private const string DesiredOutput = "FUEL";

private const long DesiredOutputCount1 = 1;
private const long BaseInputAvailableCount2 = 1_000_000_000_000;

private static (long Result1, long Result2) Solve(IReadOnlyDictionary<string, Blueprint> input)
{
    long singleOutputBaseInputDemand = GetBaseInputDemand(input, DesiredOutputCount1);
    long left = BaseInputAvailableCount2 / singleOutputBaseInputDemand;
    long leftInputDemand = GetBaseInputDemand(input, desiredOutputCount: left);
    long right = ((BaseInputAvailableCount2 + leftInputDemand - 1) / leftInputDemand) * left;
    
    while (left != right)
    {
        long current = (left + right + 1) / 2;
        
        if (GetBaseInputDemand(input, desiredOutputCount: current) <= BaseInputAvailableCount2)
        {
            left = current; 
        }
        else
        {
            right = Math.Max(left, current - 1);
        }
    }
    
    return (Result1: singleOutputBaseInputDemand, Result2: left);
}

private static long GetBaseInputDemand(IReadOnlyDictionary<string, Blueprint> input, long desiredOutputCount)
{
    var dependencyMatrix = new Dictionary<string, (int Complexity, HashSet<string> Dependencies)>
    {
        [BaseInput] = (Complexity: 0, Dependencies: [])
    };
    _ = GetDependencies(output: DesiredOutput);

    var demands = new Dictionary<string, long>
    {
        [DesiredOutput] = desiredOutputCount
    };
    
    for (int c = dependencyMatrix[DesiredOutput].Complexity; c >= 1; c--)
    {
        IReadOnlyDictionary<string, long> ingredientsToProduce =
            demands
                .Where(kv => dependencyMatrix[kv.Key].Complexity == c)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        
        foreach ((string materialToProduce, long countToProduce) in ingredientsToProduce)
        {
            demands.Remove(materialToProduce);
            (int outputCount, IReadOnlyDictionary<string, int> inputs) = input[materialToProduce];
            long productionRoundCount = (countToProduce + outputCount - 1) / outputCount;
        
            foreach ((string ingredientName, int ingredientCount) in inputs)
            {
                if (!demands.ContainsKey(ingredientName))
                {
                    demands[ingredientName] = 0;
                }
                
                demands[ingredientName] += productionRoundCount * ingredientCount;
            }
        }
    }
    
    return demands[BaseInput];
    
    (int Complexity, HashSet<string> Dependencies) GetDependencies(string output)
    {
        if (dependencyMatrix.TryGetValue(output, out (int Complexity, HashSet<string> Dependencies) result))
        {
            return result;
        }
        
        int complexity = 0;
        HashSet<string> dependencies = new HashSet<string>();
        
        foreach (string inputMaterial in input[output].Inputs.Keys)
        {
            (int inputComplexity, HashSet<string> inputDependencies) = GetDependencies(inputMaterial);
            complexity = Math.Max(complexity, inputComplexity + 1);
            dependencies.UnionWith(inputDependencies.Append(inputMaterial));
        }
        
        dependencyMatrix[output] = (complexity, dependencies);
        return dependencyMatrix[output];
    }
}

private static IReadOnlyDictionary<string, Blueprint> ParseInput(IEnumerable<string> input)
{
    var result = new Dictionary<string, Blueprint>();

    foreach (string line in input)
    {
        string[] lineParts = line.Split(" => ");
        (int outputCount, string outputMaterial) = ParseIngredient(lineParts[1]);
        IReadOnlyDictionary<string, int> inputs =
            lineParts[0]
                .Split(", ")
                .Select(ParseIngredient)
                .ToDictionary(t => t.Material, t => t.Count);
        result.Add(outputMaterial, new Blueprint(Count: outputCount, Inputs: inputs));
    }

    return result;
    
    (int Count, string Material) ParseIngredient(string line)
    {
        string[] lineParts = line.Split(" ");
        
        return (Count: int.Parse(lineParts[0]), Material: lineParts[1]);
    }
}

private record struct Blueprint(int Count, IReadOnlyDictionary<string, int> Inputs);

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
