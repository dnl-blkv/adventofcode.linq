<Query Kind="Program" />

void Main()
{
    string input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static IReadOnlyDictionary<char, (int i, int j)> DirectionMap =
    new Dictionary<char, (int i, int j)>
    {
        ['>'] = (i:  0, j:  1),
        ['v'] = (i:  1, j:  0),
        ['<'] = (i:  0, j: -1),
        ['^'] = (i: -1, j:  0),
    };

private static int Solve1(string input) => Solve(input, agentCount: 1);

private static int Solve2(string input) => Solve(input, agentCount: 2);

private static int Solve(string input, int agentCount)
{
    (int i, int j)[] currentPositions = new (int i, int j)[agentCount];
    var visited = new HashSet<(int i, int j)>
    {
        currentPositions[0]
    };
    
    for (int i = 0; i < input.Length; i++)
    {
        (int iD, int jD) = DirectionMap[input[i]];
        int currentAgent = i % currentPositions.Length;
        currentPositions[currentAgent] = (
            i: currentPositions[currentAgent].i + iD,
            j: currentPositions[currentAgent].j + jD);
        visited.Add(currentPositions[currentAgent]);
    }
    
    return visited.Count;
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}