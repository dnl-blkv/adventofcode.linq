<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (int T, int S)[] input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private static long Solve1((int T, int S)[] input) =>
    Solve(input);

private static long Solve2((int T, int S)[] input) =>
    Solve(input.Append((T: 11, S: 0)).ToArray());

private static long Solve((int T, int S)[] input)
{
    long result = 0;
    long step = 1;
    
    foreach (((int div, int s), int i) in input.Select((t, i) => (t, i)))
    {
        int rem = (div * input.Length - s - i - 1) % div;
    
        while (result % div != rem)
        {
            result += step;
        }
        
        step *= div;
    }
    
    return result;
}

private static (int T, int S)[] ParseInput(IEnumerable<string> input) =>
    input.Select(ParseLine).ToArray();
    
private static (int T, int S) ParseLine(string line)
{
    string[] lineParts = line.Split(' ');
    
    return (T: int.Parse(lineParts[3]), S: int.Parse(lineParts[^1][..^1]));
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
