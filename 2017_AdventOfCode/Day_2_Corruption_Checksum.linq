<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int[][] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static long Solve1(int[][] input) =>
    input.Sum(line => line.Max() - line.Min());

private static long Solve2(int[][] input) =>
    input.Sum(line =>
    {
        for (int i = 0; i < line.Length; i++)
        {
            for (int j = i + 1; j < line.Length; j++)
            {
                int[] vS = [line[i], line[j]];
                (int max, int min) = (vS.Max(), vS.Min());
            
                if (max % min != 0)
                {
                    continue;
                }
                
                return max / min;
            }
        }
            
        throw new ArgumentException("The line does not have such A and B that A % B == 0!!!");
    });

private static int[][] ParseInput(IEnumerable<string> input) => input
    .Select(line => line
        .Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToArray())
    .ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}