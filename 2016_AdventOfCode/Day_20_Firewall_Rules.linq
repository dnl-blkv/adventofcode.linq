<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (long From, long To)[] input = ParseInput(GetInput());
    Solve(input).Dump();
}

private const long RangeStart = 0;
private const long RangeEnd = 4294967295;

private static (long Result1, long Result2) Solve((long From, long To)[] input)
{
    long? result1 = null;
    long result2 = RangeStart;
    long current = 0;
    
    foreach ((long from, long to) in input.OrderBy(t => t).Where(t => t.To >= current))
    {
        if (current < from)
        {
            result1 ??= current;
            result2 += from - current;
        }
        
        current = to + 1;
    }
    
    if (current < RangeEnd)
    {
        result2 += RangeEnd - current;
    }
    
    return (Result1: result1!.Value, Result2: result2);
}

private static (long From, long To)[] ParseInput(IEnumerable<string> input) =>
    input
        .Select(
            line =>
            {
                long[] lineParts = line.Split('-').Select(long.Parse).ToArray();
                
                return (From: lineParts[0], To: lineParts[1]);
            })
        .ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
