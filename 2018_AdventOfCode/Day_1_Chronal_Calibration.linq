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

private static long Solve1(int[] input) => input.Sum();

private static long Solve2(int[] input)
{
    int result = 0;
    int i = 0;
    HashSet<int> seen = [];
    
    while (true)
    {
        result += input[i++ % input.Length];
        
        if (seen.Add(result))
        {
            continue;
        }
        
        return result;
    }
}

private static int[] ParseInput(IEnumerable<string> input) => input.Select(int.Parse).ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
