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

private static long Solve1(int[] input) =>
    Solve(input, getDelta: _ => 1);

private static long Solve2(int[] input) =>
    Solve(input, getDelta: val => (val < 3 ? 1 : -1));

private static long Solve(int[] input, Func<int, int> getDelta)
{
    var state = new int[input.Length];
    input.CopyTo(state, index: 0);
    
    int result = 0;
    
    for (int pos = 0; 0 <= pos && pos < state.Length; result++)
    {
        int prevPos = pos;
        pos += state[pos];
        state[prevPos] += getDelta(state[prevPos]);
    }
    
    return result;
}

private static int[] ParseInput(IEnumerable<string> input) =>
    input.Select(int.Parse).ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}