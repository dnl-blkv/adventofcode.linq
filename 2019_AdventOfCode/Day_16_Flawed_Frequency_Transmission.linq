<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<int> input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int BlockSizeOffset1 = 0;
private const int MessageOffsetElementCount2 = 7;
private const int InputMultiplier2 = 10_000;

private const int PhaseCount = 100;
private const int BlockPartCount = 4;
private const int PlusBlockPartAdjustedIndex = 0;
private const int MinusBlockPartAdjustedIndex = 2;
private const int DecimalBase = 10;
private const int ResultElementCount = 8;

private static string Solve1(IReadOnlyList<int> input) =>
    Solve(input, BlockSizeOffset1);

private static string Solve2(IReadOnlyList<int> input)
{
    int messageOffset =
        int.Parse(string.Join(string.Empty, input.Take(MessageOffsetElementCount2)));
    IEnumerable<int> extendedInput =
        Enumerable.Range(0, input.Count * InputMultiplier2 - messageOffset)
            .Select(i => input[(i + messageOffset) % input.Count]);

    return Solve(extendedInput, blockSizeOffset: messageOffset);
}

private static string Solve(IEnumerable<int> input, int blockSizeOffset)
{
    int[] currentSignal = input.ToArray();
    int[] firstNSums = GetFirstNSums(currentSignal);

    for (int i = 0; i < PhaseCount; i++)
    {
        var nextSignal = new int[currentSignal.Length];
     
        for (int j = 1; j <= currentSignal.Length; j++)
        {
            int nextSignalElementSum = 0;
            int blockPartLength = j + blockSizeOffset;
            int plusBlockPartOffset = PlusBlockPartAdjustedIndex * blockPartLength;
            int minusBlockPartOffset = MinusBlockPartAdjustedIndex * blockPartLength;
        
            for (int k = j - 1; k < currentSignal.Length; k += blockPartLength * BlockPartCount)
            {
                nextSignalElementSum +=
                    GetPartialSum(from: k + plusBlockPartOffset, length: blockPartLength)
                    - GetPartialSum(from: k + minusBlockPartOffset, length: blockPartLength);
            }
            
            nextSignal[j - 1] = Math.Abs(nextSignalElementSum) % DecimalBase;
        }
        
        currentSignal = nextSignal;
        firstNSums = GetFirstNSums(currentSignal);
    }
    
    return string.Join(string.Empty, currentSignal.Take(ResultElementCount));
    
    int GetPartialSum(int from, int length) =>
        firstNSums[Math.Min(currentSignal.Length, Math.Max(0, from))]
        - firstNSums[Math.Min(currentSignal.Length, from + length)];
}
    
private static int[] GetFirstNSums(int[] enumerable)
{
    var result = new int[enumerable.Length + 1];
    int rollingSum = 0;
    result[0] = rollingSum;
    
    for (int i = 0; i < enumerable.Length; i++)
    {
        result[i + 1] = rollingSum += enumerable[i];
    }
    
    return result;
}

private static IReadOnlyList<int> ParseInput(IEnumerable<string> input) =>
    input.Single().Select(c => c - '0').ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
