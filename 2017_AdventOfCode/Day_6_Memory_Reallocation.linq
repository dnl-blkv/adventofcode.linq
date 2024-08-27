<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int[] input = ParseInput(GetInput());
	Solve(input).Dump();
}

private static (long Result1, long Result2) Solve(int[] input)
{
    var state = new int[input.Length];
    input.CopyTo(state, 0);
    
    return (Result1: StepUntilRepeat(input), Result2: StepUntilRepeat(input));
}

private static long StepUntilRepeat(int[] state)
{
    int stepCount = 0;
    HashSet<string> visited = [];
    
    while (visited.Add(ToHashable(state)))
    {
        Step(state);
        stepCount++;
    }
    
    return stepCount;
}

private static void Step(int[] state)
{
    int indexOfMax = FindIndexOfMax(state);
    int valOfMax = state[indexOfMax];
    int fullAdd = valOfMax / state.Length;
    state[indexOfMax] = 0;
    
    for (int i = 0; i < state.Length; i++)
    {
        state[i] += fullAdd;
    }
    
    int remAddCount = valOfMax % state.Length;
    
    for (int i = 0; i < remAddCount; i++)
    {
        state[(indexOfMax + i + 1) % state.Length]++;
    }
}

private static string ToHashable(int[] state) =>
    string.Join(',', state);

private static int FindIndexOfMax(int[] input) =>
    Array.IndexOf(input, input.Max());

private static int[] ParseInput(IEnumerable<string> input) =>
    input.Single()
        .Split([" ", "\t"], StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}