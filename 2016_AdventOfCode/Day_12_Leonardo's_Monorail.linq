<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    BunnyProgram input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static long Solve1(BunnyProgram input) => Solve(input);

private static long Solve2(BunnyProgram input) => Solve(input, c: 1);

private static long Solve(BunnyProgram input, int c = 0) => input.Execute(c: c)["a"];

private static BunnyProgram ParseInput(IEnumerable<string> input) => BunnyProgram.Compile(input);

private class BunnyProgram(IReadOnlyList<Func<IDictionary<string, long>, int, int>> actions)
{
    private readonly IReadOnlyList<Func<IDictionary<string, long>, int, int>> actions = actions;
    
    public static BunnyProgram Compile(IEnumerable<string> instructions)
    {
        var actions = new List<Func<IDictionary<string, long>, int, int>>();
    
        foreach (string instruction in instructions)
        {
            string[] instructionParts = instruction.Split(' ');
            string type = instructionParts[0];
            string arg1 = instructionParts[1];
            int? arg1Val = (int.TryParse(arg1, out int val) ? val : null);
            string? arg2 = (instructionParts.Length > 2 ? instructionParts[2] : null);
            
            switch (type)
            {
                case "cpy":
                    actions.Add((memory, i) =>
                    {
                        memory[arg2!] = arg1Val ?? memory[arg1];
                        return i + 1;
                    });
                    break;
                    
                case "inc":
                    actions.Add((memory, i) =>
                    {
                        memory[arg1]++;
                        return i + 1;
                    });
                    break;
                    
                case "dec":
                    actions.Add((memory, i) =>
                    {
                        memory[arg1]--;
                        return i + 1;
                    });
                    break;
                    
                case "jnz":
                    int arg2Val = int.Parse(arg2!);
                    actions.Add((memory, i) => i + ((arg1Val ?? memory[arg1]) != 0 ? arg2Val : 1));
                    break;
                    
                default:
                    throw new ArgumentException($"Uknown instruction type: '{type}'");
            }
        }
        
        return new BunnyProgram(actions);
    }
    
    public IReadOnlyDictionary<string, long> Execute(int a = 0, int b = 0, int c = 0, int d = 0)
    {
        var memory = new Dictionary<string, long>
        {
            ["a"] = a,
            ["b"] = b,
            ["c"] = c,
            ["d"] = d
        };
        int i = 0;
        
        while (i >= 0 && i < actions.Count)
        {
            i = actions[i].Invoke(memory, i);
        }
        
        return memory;
    }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}