<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int[] input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int MaxSteps1 = 1;
private const int MaxSteps2 = int.MaxValue;

private static long Solve1(int[] input) => Solve(input, MaxSteps1);

private static long Solve2(int[] input) => Solve(input, MaxSteps2);

private static long Solve(int[] input, int maxSteps) =>
    input.Sum(n => CalculateFuelRequirement(n, maxSteps));

private static int CalculateFuelRequirement(int n, int maxSteps)
{
    int result = 0;
    int rem = n;
    
    for (int i = 0; i < maxSteps && rem > 0; i++)
    {
        rem = rem / 3 - 2;
        result += Math.Max(0, rem);
    }
    
    return result;
}

private static int[] ParseInput(IEnumerable<string> input) => input.Select(int.Parse).ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
