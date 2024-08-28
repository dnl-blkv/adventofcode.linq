<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int input = ParseInput(GetInput());
    Solve(input).Dump();
}

private const int ScoreLength1 = 10;
private const int DecimalBase = 10;

private static readonly IReadOnlyList<int> StartList = [3, 7];

private static (long Result1, int Result2) Solve(int input)
{
    List<int> list = StartList.ToList();
    
    int p0 = 0;
    int p1 = 1;
    
    int stopAtLength1 = input + ScoreLength1;
    long result1 = -1;
    
    int scoreLength2 = GetDigitsReversed(input).Count();
    long rollingScore = -1;
    int rollingScoreMod = (int)Math.Pow(DecimalBase, scoreLength2 - 1);
    int result2 = -1;
    
    while (result1 < 0 || result2 < 0)
    {
        int v0 = list[p0];
        int v1 = list[p1];
        
        int oldCount = list.Count;
        list.AddRange(GetDigits(v0 + v1));
        
        TryUpdateResult1IfNeeded();
        TryUpdateResult2IfNeeded(oldCount);
        
        p0 = (p0 + v0 + 1) % list.Count;
        p1 = (p1 + v1 + 1) % list.Count;
    }
    
    return (Result1: result1, Result2: result2);
    
    void TryUpdateResult1IfNeeded()
    {
        if (result1 >= 0 || list.Count < stopAtLength1)
        {
            return;
        }
    
        result1 = GetScore(startAt: input, endBefore: stopAtLength1);
    }
    
    long GetScore(int startAt, int endBefore)
    {
        long mul = 1;
        long result = 0;
        
        for (int i = endBefore - 1; i >= startAt; i--)
        {
            result += mul * list[i];
            mul *= DecimalBase;
        }
        
        return result;
    }
    
    void TryUpdateResult2IfNeeded(int oldCount)
    {
        if (result2 >= 0)
        {
            return;
        }
        
        for (int i = Math.Max(oldCount, scoreLength2 - 1); i < list.Count; i++)
        {
            if (rollingScore < 0)
            {
                int endBefore = i + 1;
                int startAt = endBefore - scoreLength2;
                rollingScore = GetScore(startAt: startAt, endBefore: endBefore);
            }
            else
            {
                rollingScore = (rollingScore % rollingScoreMod) * DecimalBase + list[i];
            }
        
            if (rollingScore != input)
            {
                continue;
            }
            
            result2 = i + 1 - scoreLength2;
        }
    }
}

private static IEnumerable<int> GetDigits(int n) => GetDigitsReversed(n).Reverse();

private static IEnumerable<int> GetDigitsReversed(int n)
{
    if (n == 0)
    {
        yield return 0;
        yield break;
    }

    int rem = n;
    
    while (rem > 0)
    {
        yield return rem % DecimalBase;
        rem /= DecimalBase;
    }
}

private static int ParseInput(IEnumerable<string> input) => int.Parse(input.Single());

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
