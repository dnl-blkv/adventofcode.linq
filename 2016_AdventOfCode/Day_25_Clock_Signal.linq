<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    BunnyProgram input = ParseInput(GetInput());
    Solve(input).Dump();
}

private const string Out = "out";

private static long Solve(BunnyProgram input) => input.FindClockA();

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
                    
                case Out:
                    actions.Add((memory, i) =>
                    {
                        memory[Out] = arg1Val ?? memory[arg1];
                        return i + 1;
                    });
                    break;
                    
                default:
                    throw new ArgumentException($"Uknown instruction type: '{type}'");
            }
        }
        
        return new BunnyProgram(actions);
    }
    
    public int FindClockA()
    {
        for (int a = 0; a < int.MaxValue; a++)
        {
            var memory = new Dictionary<string, long>
            {
                ["a"] = a,
                ["b"] = 0,
                ["c"] = 0,
                ["d"] = 0
            };
            HashSet<(long A, long B, long C, long D, int O)> visited = [];
            
            for (int i = 0, cnt = 0; i >= 0 && i < actions.Count; i = actions[i].Invoke(memory, i))
            {
                if (!memory.TryGetValue(Out, out long outVal))
                {
                    continue;
                }
                
                memory.Remove("out");
                
                int o = cnt % 2;
                
                if (outVal != o)
                {
                    break;
                }
                
                if (!visited.Add(CreateKey(memory, o)))
                {
                    return a;
                }
            
                cnt++;
            }
        }
        
        return -1;
    }
    
    private static (long A, long B, long C, long D, int O) CreateKey(IReadOnlyDictionary<string, long> mem, int o) =>
        (A: mem["a"], B: mem["b"], C: mem["c"], D: mem["d"], O: o);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
