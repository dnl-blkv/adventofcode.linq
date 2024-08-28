<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    long input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private static long Solve1(long input)
{
    Num current = CreateCircle(input);
    
    while (current.Next != current)
    {
        Num next = current.Next!.Next!;
        (current.Next, next.Prev) = (next, current);
        current = next;
    }
    
    return current.Value;
}

private static long Solve2(long input)
{
    Num current = CreateCircle(input);
    Num victim = current;
    
    for (int i = 0; i < input / 2; i++)
    {
        victim = victim.Next!;
    }
    
    long size = input;
    
    while (victim != current)
    {
        victim = victim.Next!;
        Num prev = victim.Prev!.Prev!;
        (victim.Prev, prev.Next) = (prev, victim);
        
        current = current.Next!;
        
        if (--size % 2 == 1)
        {
            continue;
        }
        
        victim = victim.Next!;
    }
    
    return current.Value;
}

private static Num CreateCircle(long size)
{
    Num start = new Num(1);
    Num current = start;
    
    for (int i = 2; i <= size; i++)
    {
        var next = new Num(i);
        (current.Next, next.Prev) = (next, current);
        current = next;
    }
    
    (current.Next, start.Prev) = (start, current);
    
    return start;
}

private static long ParseInput(IEnumerable<string> input) => long.Parse(input.Single());

private class Num(int value)
{
    public int Value { get; } = value;
    
    public Num? Prev { get; set; }
    
    public Num? Next { get; set; }
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
