<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyDictionary<string, Program> input = ParseInput(GetInput());
	Solve(input).Dump();
}

private static (string Result1, int Result2) Solve(IReadOnlyDictionary<string, Program> input)
{
    string result1 = input.Keys.Except(input.Values.SelectMany(p => p.Children)).Single();
    
    Dictionary<string, int> fullWeights = [];
    _ = CalcWeights(result1);
    
    string badNodeName = result1;
    
    while (true)
    {
        IReadOnlyDictionary<int, Program> oddChildren = GetOddChildren(badNodeName);
        
        if (oddChildren.Count == 0)
        {
            break;
        }
        
        badNodeName = oddChildren.FirstOrDefault().Value.Name;
    }
    
    Program badCandidateParentNode = input.Values.Single(p => p.Children.Contains(badNodeName));
    string goodSiblingName = badCandidateParentNode.Children.First(c => c != badNodeName);
    int result2 = input[badNodeName].Weight + fullWeights[goodSiblingName] - fullWeights[badNodeName];
    
    return (Result1: result1, Result2: result2);
    
    int CalcWeights(string nodeName)
    {
        int result = input[nodeName].Weight + input[nodeName].Children.Sum(CalcWeights);
        fullWeights[nodeName] = result;
        
        return fullWeights[nodeName];
    }
    
    IReadOnlyDictionary<int, Program> GetOddChildren(string nodeName) =>
        input[nodeName].Children.Select(n => input[n]).GroupBy(p => fullWeights[p.Name])
            .Select(g => (Key: g.Key, Items: g.ToArray()))
            .Where(t => t.Items.Length is 1)
            .ToDictionary(t => t.Key, t => t.Items.Single());
}

private static IReadOnlyDictionary<string, Program> ParseInput(IEnumerable<string> input) =>
    input.Select(Program.Parse).ToDictionary(p => p.Name, p => p);

private record struct Program(string Name, int Weight, IReadOnlyList<string> Children)
{
    public static Program Parse(string programString)
    {
        string[] programStringParts = programString.Split(" -> ");
        string[] programDetails = programStringParts[0].Split(" ");
        string[] programChildren =
            programStringParts is [_, string programChildrenString]
                ? programChildrenString.Split(", ")
                : Array.Empty<string>();
        
        return new Program(
            Name: programDetails[0],
            Weight: int.Parse(programDetails[1].Trim(['(', ')'])),
            Children: programChildren);
    }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}