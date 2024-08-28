<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private static long Solve1(string input) => Solve(input, dist: 1);

private static long Solve2(string input) => Solve(input, dist: input.Length / 2);

private static long Solve(string input, int dist) =>
    input.Select((c, i) => (c == input[(i + dist) % input.Length] ? c - '0' : 0)).Sum();

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
