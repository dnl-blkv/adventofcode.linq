<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static long Solve1(string[] input)
{
    IReadOnlyList<IReadOnlyDictionary<char, int>> letterCounts =
        input.Select(GetLetterCounts).ToArray();
    int twos = letterCounts.Count(lc => lc.Values.Contains(2));
    int threes = letterCounts.Count(lc => lc.Values.Contains(3));
    
    return twos * threes;
}

private static string Solve2(string[] input)
{
    for (int i = 0; i < input.Length; i++)
    {
        for (int j = i + 1; j < input.Length; j++)
        {
            (bool IsEqual, int Pos)[] diff =
                input[i].Zip(input[j])
                    .Select((t, pos) => (IsEqual: t.First == t.Second, Pos: pos))
                    .Where(t => !t.IsEqual)
                    .Take(2)
                    .ToArray();
            
            if (diff.Length > 1)
            {
                continue;
            }
            
            ReadOnlySpan<char> str = input[i].AsSpan();
            int pos = diff.Single().Pos;
            
            return string.Concat(str[..pos], str[(pos + 1)..]);
        }
    }
    
    return string.Empty;
}

private static IReadOnlyDictionary<char, int> GetLetterCounts(string input)
{
    Dictionary<char, int> result = [];
    
    foreach (char c in input)
    {
        if (!result.ContainsKey(c))
        {
            result[c] = 0;
        }
        
        result[c]++;
    }
    
    return result;
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}