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

private const int RowCount1 = 40;
private const int RowCount2 = 400000;
private const char SafeChar = '.';
private const char TrapChar = '^';

private static long Solve1(string input) => Solve(input, RowCount1);

private static long Solve2(string input) => Solve(input, RowCount2);

private static long Solve(string input, int rowCount) =>
    Enumerable
        .Range(0, rowCount)
        .Aggregate(
            (Row: input, Count: 0),
            (t, _) => (Row: GetNextRow(t.Row), Count: t.Count + t.Row.Count(IsSafe))).Count;

private static string GetNextRow(string row)
{
    var nextRowBuilder = new StringBuilder();
    
    for (int i = 0; i < row.Length; i++)
    {
        bool leftSafe = (i is 0 || IsSafe(row[i - 1]));
        bool rightSafe = (i >= row.Length - 1 || IsSafe(row[i + 1]));
        nextRowBuilder.Append((leftSafe ^ rightSafe) ? TrapChar : SafeChar);
    }
    
    return nextRowBuilder.ToString();
}

private static bool IsSafe(char t) => t is SafeChar;

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}