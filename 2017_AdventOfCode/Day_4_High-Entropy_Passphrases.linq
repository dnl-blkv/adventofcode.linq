<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[][] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static long Solve1(string[][] input) =>
    Solve(input);

private static long Solve2(string[][] input) =>
    Solve(input.Select(l => l.Select(s => string.Join(string.Empty, s.OrderBy(c => c))).ToArray()));

private static long Solve(IEnumerable<string[]> input) =>
    input.Count(a => a.Length == a.Distinct().Count());

private static string[][] ParseInput(IEnumerable<string> input) =>
    input.Select(l => l.Split(' ')).ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}