<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<int> input = ParseInput(GetInput());
    Solve(input).Dump();
}

private static (long Result1, long Result2) Solve(IReadOnlyList<int> input)
{
    int inputSum = input.Sum();
    int targetSum1 = inputSum / 3;
    int targetSum2 = inputSum / 4;
    IReadOnlyList<int[]>[] pathsUpToTargetSum = GetAllPathsUpTo(Math.Max(targetSum1, targetSum2));
    
    return (Result1: Solve(input, targetSum1), Result2: Solve(input, targetSum2));
            
    IReadOnlyList<int[]>[] GetAllPathsUpTo(int max)
    {
        List<int[]>[] resultTable = Enumerable.Range(0, max + 1).Select(_ => new List<int[]>()).ToArray();
        
        for (int j = 0; j < input.Count; j++)
        {
            int n = input[j];
            resultTable[n].Add(new int[] { j });
        }
        
        for (int i = input[0]; i < max + 1; i++)
        {
            foreach (int[] bags in resultTable[i])
            {
                for (int j = bags[^1] + 1; j < input.Count; j++)
                {
                    int n = input[j];
                    
                    if (i + n >= resultTable.Length)
                    {
                        break;
                    }
                    
                    resultTable[i + n].Add(bags.Append(j).ToArray());
                }
            }
        }
        
        return resultTable;
    }
        
    long Solve(IReadOnlyList<int> input, int targetSum) =>
        pathsUpToTargetSum[targetSum]
            .Select(bags => bags.Select(k => input[k]).ToArray())
            .Min(bags => (L: bags.Length, Qe: bags.Aggregate(1L, (qe, s) => qe * s)))!.Qe;
}

private static IReadOnlyList<int> ParseInput(IEnumerable<string> input) => input.Select(int.Parse).ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
