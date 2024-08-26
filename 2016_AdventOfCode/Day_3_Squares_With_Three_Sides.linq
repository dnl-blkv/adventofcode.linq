<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (int A, int B, int C)[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static long Solve1((int A, int B, int C)[] input) =>
    CountValidTriangles(input);

private static long Solve2((int A, int B, int C)[] input) =>
    CountValidTriangles(DecodeVerticalTriangles(input));

private static long CountValidTriangles(IEnumerable<(int A, int B, int C)> input) =>
    input.Count(t => t.A < t.B + t.C && t.B < t.A + t.C && t.C < t.A + t.B);
    
private static IEnumerable<(int A, int B, int C)> DecodeVerticalTriangles((int A, int B, int C)[] input)
{
    for (int i = 0; i < input.Length; i += 3)
    {
        yield return (A: input[i].A, B: input[i + 1].A, C: input[i + 2].A);
        yield return (A: input[i].B, B: input[i + 1].B, C: input[i + 2].B);
        yield return (A: input[i].C, B: input[i + 1].C, C: input[i + 2].C);
    }
}

private static (int A, int B, int C)[] ParseInput(IEnumerable<string> input) =>
    input
        .Select(l => l
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray())
        .Select(nss => (A: nss[0], B: nss[1], C: nss[2]))
        .ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}