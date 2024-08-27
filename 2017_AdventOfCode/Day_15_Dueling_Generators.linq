<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    long[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private const int BinBase = 2;
private const int SignificantSuffixLength = 16;
private const int IterationCount1 = 40_000_000;
private const int IterationCount2 = 5_000_000;

private const long GeneratorAFactor = 16807;
private const long GeneratorBFactor = 48271;
private const long GeneratorDivisor = int.MaxValue;
private const long Div2A = 4;
private const long Div2B = 8;

private static readonly int LongBitLength = Convert.ToString(long.MaxValue, BinBase).Length;

private static long Solve1(long[] input) =>
    Solve(input, iterationCount: IterationCount1, filterA: _ => true, filterB: _ => true);

private static long Solve2(long[] input) =>
    Solve(input, iterationCount: IterationCount2, filterA: n => n % Div2A == 0, filterB: n => n % Div2B == 0);

private static long Solve(
    long[] input,
    int iterationCount,
    Func<long, bool> filterA,
    Func<long, bool> filterB)
{
    long result = 0;
    long a = input[0];
    long b = input[1];
    
    for (int i = 0; i < iterationCount; i++)
    {
        a = GetNextValue(value: a, factor: GeneratorAFactor, filter: filterA);
        b = GetNextValue(value: b, factor: GeneratorBFactor, filter: filterB);
        
        if (GetSignificantSuffix(a) != GetSignificantSuffix(b))
        {
            continue;
        }
        
        result++;
    }
    
    return result;
}

private static long GetNextValue(long value, long factor, Func<long, bool> filter)
{
    long result = value;
    
    do 
    {
        result = (result * factor) % GeneratorDivisor;
    } while (!filter.Invoke(result));
    
    return result;
}

private static string GetSignificantSuffix(long value) =>
    Convert.ToString(value, BinBase).PadLeft(SignificantSuffixLength, '0')[^SignificantSuffixLength..];

private static long[] ParseInput(IEnumerable<string> input) =>
    input.Select(s => s.Split(' ').Last()).Select(long.Parse).ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}