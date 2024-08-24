<Query Kind="Program" />

void Main()
{
    string input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static int Solve1(string input) =>
    input.Aggregate(0, (t, c) => t += GetFloorDelta(c));
    
private static int GetFloorDelta(char c) => 1 - 2 * (c - '(');

private static int Solve2(string input)
{
    int t = 0;
	return input
        .Select((c, i) => (c, i))
        .First(p => (t += GetFloorDelta(p.c)) < 0)
        .i + 1;
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}