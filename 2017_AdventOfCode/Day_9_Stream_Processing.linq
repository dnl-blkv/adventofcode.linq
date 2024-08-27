<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
	Solve(input).Dump();
}

private static (long Result1, long Result2) Solve(string input)
{
    int result1 = 0;
    int level = 0;
    
    (IReadOnlyList<char> good, int result2) = SortGarbage(input);
    
    foreach (char c in good)
    {
        switch (c)
        {
            case '{':
                level++;
                break;
                
            case '}':
                result1 += level;
                level--;
                break;
                
            default:
                break;
        }
    }
    
    return (result1, result2);
}

private static (IReadOnlyList<char> Good, int badCount) SortGarbage(string input)
{
    bool isGarbage = false;
    var good = new List<char>();
    int badCount = 0;
    
    for (int i = 0; i < input.Length; i++)
    {
        if (input[i] == '<')
        {
            badCount += (isGarbage ? 1 : 0);
            isGarbage = true;
            
            continue;
        }
        
        if (!isGarbage)
        {
            good.Add(input[i]);
            continue;
        }
        
        switch (input[i])
        {
            case '!':
                i++;
                break;
                
            case '>':
                isGarbage = false;
                break;
                
            default:
                badCount++;
                break;
        }
    }
    
    return (good, badCount);
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}