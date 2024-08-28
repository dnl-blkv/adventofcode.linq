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

private static long Solve1(ReadOnlySpan<char> input) => Solve(input, recurse: false);

private static long Solve2(ReadOnlySpan<char> input) => Solve(input, recurse: true);

private static long Solve(ReadOnlySpan<char> input, bool recurse)
{
    long result = 0;
    
    for (int i = 0; i < input.Length; i++)
    {
        if (input[i] is not '(')
        {
            result++;
            continue;
        }
        
        int xIndex = i + input[i..].IndexOf('x');
        int len = int.Parse(input[(i + 1)..xIndex]);
        
        int nextStart = xIndex + input[xIndex..].IndexOf(')');
        int repetitionCount = int.Parse(input[(xIndex + 1)..nextStart]);
        i = nextStart;
        
        long subResult = recurse
            ? Solve(input[(i + 1)..(i + 1 + len)], recurse)
            : len;
        result += subResult * repetitionCount;
        i += len;
    }
    
    return result;
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
