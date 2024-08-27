<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
	Solve(input).Dump();
}

private static (long Result1, long Result2) Solve(string[] input)
{
    var memory = new Dictionary<string, long>();
    long result2 = input
        .Select(line => CompileInstruction(line, memory))
        .Aggregate(0L, (rMax, instruction) => Math.Max(rMax, instruction()));
    
    return (Result1: memory.Values.Max(), Result2: result2);
}
    
private static Func<long> CompileInstruction(string line, Dictionary<string, long> memory)
{
    string[] lineParts = line.Split(' ');
    string targetName = lineParts[0];
    long delta = (lineParts[1] == "inc" ? 1 : -1) * long.Parse(lineParts[2]);
    string conditionLeftName = lineParts[4];
    Func<long, long, bool> operation =
        lineParts[5] switch
        {
            "<" => Less,
            "<=" => LessOrEqual,
            "==" => Equal,
            "!=" => NotEqual,
            ">=" => GreaterOrEqual,
            ">" => Greater,
            _ => throw new InvalidOperationException("Unknown Operation!")
        };
    long conditionRightValue = long.Parse(lineParts[6]);
    
    return (Func<long>)(() =>
    {
        if (!memory.TryGetValue(conditionLeftName, out long conditionLeftValue))
        {
            memory[conditionLeftName] = 0;
        }
        
        if (!memory.ContainsKey(targetName))
        {
            memory[targetName] = 0;
        }
    
        if (operation(conditionLeftValue, conditionRightValue))
        {
            memory[targetName] += delta;
        }
        
        return memory[targetName];
    });
    
    static bool Less(long a, long b) => a < b;
    
    static bool LessOrEqual(long a, long b) => a <= b;
    
    static bool Equal(long a, long b) => a == b;
    
    static bool NotEqual(long a, long b) => a != b;
    
    static bool GreaterOrEqual(long a, long b) => a >= b;
    
    static bool Greater(long a, long b) => a > b;
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}