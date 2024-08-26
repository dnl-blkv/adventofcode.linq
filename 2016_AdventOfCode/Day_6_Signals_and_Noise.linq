<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static string Solve1(string[] input) =>
    Solve(input, Enumerable.Max);

private static string Solve2(string[] input) =>
    Solve(input, enumerable => enumerable.Where(ci => ci.C > 0).Min());

private static string Solve(string[] input, Func<IEnumerable<(int C, int I)>, (int C, int I)> charInfoSelector)
{
    int[][] counts =
        Enumerable
            .Range(0, input[0].Length)
            .Select(_ => Enumerable.Repeat(0, 26).ToArray())
            .ToArray();
    
    foreach (string line in input)
    {
        foreach ((char c, int i) in line.Select((c, i) => (c, i)))
        {
            counts[i][c - 'a']++;
        }
    }
    
    return string.Join(
        string.Empty,
        counts.Select(lcs => (char)(charInfoSelector.Invoke(lcs.Select((n, i) => (C: n, I: i))).I + 'a')));
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}