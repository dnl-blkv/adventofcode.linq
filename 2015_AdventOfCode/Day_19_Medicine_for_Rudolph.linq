<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (
        IReadOnlyDictionary<string, IReadOnlyList<string>> replacementTable,
        string medicineMolecule) = ParseInput(GetInput());
    Solve1(replacementTable, medicineMolecule).Dump();
    Solve2(replacementTable, medicineMolecule).Dump();
}

private static int Solve1(
    IReadOnlyDictionary<string, IReadOnlyList<string>> replacementTable,
    string medicineMolecule) =>
    GetNextGeneration(replacementTable, medicineMolecule).Distinct().Count();

private static int Solve2(
    IReadOnlyDictionary<string, IReadOnlyList<string>> replacementTable,
    string targetMolecule) =>
    GetElements(targetMolecule).Sum(e => e is "Ar" or "Y" ? -1 : 1) - 1;
    
private static IEnumerable<string> GetNextGeneration(
    IReadOnlyDictionary<string, IReadOnlyList<string>> replacementTable,
    string molecule)
{
    int[] keyLengths = replacementTable.Keys.Select(k => k.Length).Distinct().OrderBy(k => k).ToArray();
    
    for (int i = 0; i < molecule.Length; i++)
    {
        foreach (int keyLength in keyLengths)
        {
            int keyCandidateStartIndex = i - keyLength + 1;
        
            if (keyCandidateStartIndex < 0)
            {
                break;
            }
            
            string keyCandidate = molecule[keyCandidateStartIndex..(i + 1)];
        
            if (!replacementTable.TryGetValue(keyCandidate, out IReadOnlyList<string>? replacements))
            {
                continue;
            }
            
            foreach (string replacement in replacements)
            {
                yield return $"{molecule[..keyCandidateStartIndex]}{replacement}{molecule[(i + 1)..]}";
            }
        }
    }
}

private static IEnumerable<string> GetElements(IEnumerable<char> input)
{
    var elementBuilder = new StringBuilder();
    
    foreach (char c in input)
    {
        if (c == 'e')
        {
            continue;
        }
        
        if (char.IsUpper(c) && elementBuilder.Length > 0)
        {
            yield return elementBuilder.ToString();
            elementBuilder.Length = 0;
        }
    
        elementBuilder.Append(c);
    }
    
    yield return elementBuilder.ToString();
}

private static (
    IReadOnlyDictionary<string, IReadOnlyList<string>> ReplacementTable,
    string MedicineMolecule) ParseInput(IEnumerable<string> input)
    {
        bool replacementTableParsingDone = false;
        var replacementTable = new Dictionary<string, List<string>>();
        
        foreach (string line in input)
        {
            if (line.Length is 0)
            {
                replacementTableParsingDone = true;
                continue;
            }
        
            if (replacementTableParsingDone)
            {
                return (
                    ReplacementTable: replacementTable
                        .ToDictionary(kv => kv.Key, kv => (IReadOnlyList<string>)kv.Value),
                    MedicineMolecule: line);
            }
            
            string[] pair = line.Split(" => ");
            string from = pair[0];
            string to = pair[1];
            
            if (!replacementTable.ContainsKey(from))
            {
                replacementTable[from] = new List<string>();
            }
            
            replacementTable[from].Add(to);
        }
        
        throw new Exception("Should not be reachable.");
    }

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
