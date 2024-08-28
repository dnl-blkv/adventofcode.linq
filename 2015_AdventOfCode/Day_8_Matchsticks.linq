<Query Kind="Program">
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<string> input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private static int Solve1(IReadOnlyList<string> input) =>
    input.Select(StringInfo.Parse).Sum(stringInfo => stringInfo.InMemorySavedSpace);

private static int Solve2(IReadOnlyList<string> input) =>
    input.Select(StringInfo.Parse).Sum(stringInfo => stringInfo.EscapedExtraSpace);

private static IReadOnlyList<string> ParseInput(IEnumerable<string> input) => input.ToArray();

private class StringInfo
{
    public int Length { get; private init; }
    
    public int MemoryLength { get; private init; }
    
    public int EscapedLength { get; private init; }
    
    public int InMemorySavedSpace => this.Length - this.MemoryLength;
    
    public int EscapedExtraSpace => this.EscapedLength - this.Length;
    
    public static StringInfo Parse(string input) =>
        new()
        {
            Length = input.Length,
            MemoryLength = CalculateMemoryLength(input),
            EscapedLength = CalculateEscapedLength(input)
        };
        
    private static int CalculateMemoryLength(string input)
    {
        int result = 0;
        int i = 1;
        
        while (i < input.Length - 1)
        {
            result++;
            
            i += input[i] switch
            {
                not '\\' => 1,
                '\\' when input[i + 1] is '"' or '\\' => 2,
                _ => 4
            };
        }
        
        return result;
    }
    
    private static int CalculateEscapedLength(string input) =>
        input.Sum(c => c is '\\' or '"' ? 2 : 1) + 2;
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
