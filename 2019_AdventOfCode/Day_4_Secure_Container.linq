<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (int from, int to) = ParseInput(GetInput());
    Solve(from, to).Dump();
}

private static (long Result1, long Result2) Solve(int from, int to)
{
    int result1 = 0;
    int result2 = 0;

    for (int i = from; i <= to; i++)
    {
        string numberString = i.ToString();
        char last = numberString[0];
        bool nonDecreasing = true;
        int sequenceSize = 1;
        bool hasAtLeastDoubleDigit = false;
        bool hasExactlyDoubleDigit = false;
        
        for (int j = 1; j < numberString.Length; j++)
        {
            char n = numberString[j];
            
            if (n < last)
            {
                nonDecreasing = false;
                break;
            }
            
            if (n == last)
            {
                sequenceSize++;
                hasAtLeastDoubleDigit |= sequenceSize is 2;
            }
            
            if (n != last || j == numberString.Length - 1)
            {
                hasExactlyDoubleDigit |= sequenceSize is 2;
                sequenceSize = 1;
            }
            
            last = n;
        }
        
        result1 += (hasAtLeastDoubleDigit && nonDecreasing ? 1 : 0);
        result2 += (hasExactlyDoubleDigit && nonDecreasing ? 1 : 0);
    }
    
    return (Result1: result1, Result2: result2);
}

private static (int From, int To) ParseInput(IEnumerable<string> input)
{
    int[] range = input.Single().Split('-').Select(int.Parse).ToArray();
    
    return (From: range[0], To: range[1]);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
