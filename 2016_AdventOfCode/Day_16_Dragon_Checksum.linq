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

private const int TargetLength1 = 272;
private const int TargetLength2 = 35651584;

private static string Solve1(string input) => Solve(input, TargetLength1);

private static string Solve2(string input) => Solve(input, TargetLength2);

private static string Solve(string input, int targetLength)
{
    string data = input;
    
    while (data.Length < targetLength)
    {
        data = Expand(data);
    }
    
    ReadOnlySpan<char> checksum = data.AsSpan()[..targetLength];

    while (checksum.Length % 2 == 0)
    {
        checksum = GetCheckSum(checksum);
    }
    
    return new string(checksum);
}

private static string Expand(string data) =>
    $"{data}{0}{new string(data.Select(c => (char)('0' + '1' - c)).Reverse().ToArray())}";
    
private static string GetCheckSum(ReadOnlySpan<char> data)
{
    var resultBuilder = new StringBuilder();

    for (int i = 0; i < data.Length; i += 2)
    {
        resultBuilder.Append((char)('0' + (data[i] == data[i + 1] ? 1 : 0)));
    }
    
    return resultBuilder.ToString();
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
