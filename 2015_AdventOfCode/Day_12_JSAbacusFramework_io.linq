<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}
   
private static long Solve1(string input) => Solve(input);

private static long Solve2(string input) => Solve(EnumerateIncludedChars(input, FindRedObjects(input)));
   
private static long Solve(IEnumerable<char> chars)
{
    var total = 0L;
    var numberBuilder = new StringBuilder();

    foreach (char c in chars)
    {
        if (char.IsDigit(c) || c == '-')
        {
            numberBuilder.Append(c);
        }
        else if (numberBuilder.Length > 0)
        {
            total += long.Parse(numberBuilder.ToString());
            numberBuilder.Length = 0;
        }
    }
    
    return total;
}

private static IEnumerable<char> EnumerateIncludedChars(string input, IEnumerable<(int From, int To)> excludedAreas)
{
    int i = 0;
    
    foreach ((int from, int to) in excludedAreas.Append((From: input.Length, To: -1)))
    {
        while (i < from)
        {
            yield return input[i];
            i++;
        }
        
        i = to + 1;
    }
}

private static IReadOnlyList<(int From, int To)> FindRedObjects(string input)
{
    var objStack = new Stack<ObjStart>();
    var result = new List<(int From, int To)>();
    
    for (int i = 0; i < input.Length; i++)
    {
        if (input[i] == '{')
        {
            objStack.Push(
                new ObjStart
                {
                    Start = i,
                    IsRed = false
                });
        }
        else if (i >= 5 && input[(i - 5)..(i + 1)] == ":\"red\"")
        {
            objStack.Peek().IsRed = true;
        }
        else if (input[i] == '}' && objStack.Pop() is { IsRed: true, Start: int start })
        {
            while (result.Count > 0 && start < result.Last().From)
            {
                result.RemoveAt(result.Count - 1);
            }
            
            result.Add((From: start, To: i));
        }
    }
    
    return result;
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private class ObjStart
{
    public int Start { get; set; }
    
    public bool IsRed { get; set; }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}