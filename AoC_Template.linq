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

private static long Solve1(string[] input)
{
    input.Dump();
    
    return -1;
}

private static long Solve2(string[] input) => -1;

private static string[] ParseInput(IEnumerable<string> input)
{
    return input.ToArray();
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
