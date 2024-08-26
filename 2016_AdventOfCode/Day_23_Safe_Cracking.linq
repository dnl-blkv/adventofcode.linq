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

private static long Solve1(BunnyProgram input) => Solve(input, a: 7);

private static long Solve2(BunnyProgram input) => Solve(input, a: 12);

private static long Solve(BunnyProgram input, int a = 0) => input.Execute(a: a).Mem["a"];

private static BunnyProgram ParseInput(IEnumerable<string> input) => BunnyProgram.Compile(input);

private class BunnyProgram(IReadOnlyList<Func<BunnyState, int, int>> actions)
{
    private readonly IReadOnlyList<Func<BunnyState, int, int>> actions = actions;
    
    public static BunnyProgram Compile(IEnumerable<string> instructions)
    {
        var actions = new List<Func<BunnyState, int, int>>();
    
        foreach (string instruction in instructions)
        {
            string[] instructionParts = instruction.Split(' ');
            string type = instructionParts[0];
            string arg1 = instructionParts[1];
            string? arg2 = (instructionParts.Length > 2 ? instructionParts[2] : null);
            int? arg1Val = (int.TryParse(arg1, out int val1) ? val1 : null);
            int? arg2Val = (int.TryParse(arg2, out int val2) ? val2 : null);
            
            switch (type)
            {
                case "cpy":
                    actions.Add((state, i) =>
                    {
                        if (!state.IsTgl(i))
                        {
                            state.Mem[arg2!] = arg1Val ?? state.Mem[arg1];
                            return i + 1;
                        }
                        
                        return i + ((arg1Val ?? state.Mem[arg1]) != 0 ? (int)(arg2Val ?? state.Mem[arg2!]) : 1);
                    });
                    break;
                    
                case "inc":
                    actions.Add((state, i) =>
                    {
                        state.Mem[arg1] += (state.IsTgl(i) ? -1 : 1);
                        return i + 1;
                    });
                    break;
                    
                case "dec":
                    actions.Add((state, i) =>
                    {
                        state.Mem[arg1] += (state.IsTgl(i) ? 1 : -1);
                        return i + 1;
                    });
                    break;
                    
                case "jnz":
                    actions.Add(
                        (state, i) =>
                        {
                            if (state.IsTgl(i))
                            {
                                if (arg2 is ("a" or "b" or "c" or "d"))
                                {
                                    state.Mem[arg2!] = arg1Val ?? state.Mem[arg1];
                                }
                            
                                return i + 1;
                            }
                            
                            return i + ((arg1Val ?? state.Mem[arg1]) != 0 ? (int)(arg2Val ?? state.Mem[arg2!]) : 1);
                        });
                    break;
                    
                case "tgl":
                    actions.Add((state, i) =>
                    {
                        if (state.IsTgl(i))
                        {
                            state.Mem[arg1]++;
                        }
                        else
                        {
                            state.Tgl(i + (int)(arg1Val ?? state.Mem[arg1]));
                        }
                    
                        return i + 1;
                    });
                    break;
                    
                default:
                    throw new ArgumentException($"Uknown instruction type: '{type}'");
            }
        }
        
        return new BunnyProgram(actions);
    }
    
    public BunnyState Execute(int a = 0, int b = 0, int c = 0, int d = 0)
    {
        int i = 0;
        var state = new BunnyState(a: a, b: b, c: c, d: d);
        
        while (i >= 0 && i < actions.Count)
        {
            i = actions[i].Invoke(state, i);
            
            if (i != 4 || state.Mem["d"] == 0)
            {
                continue;
            }
            
            state.Mem["a"] += state.Mem["b"] * (state.Mem["d"] - 1);
            state.Mem["d"] = 1;
        }
        
        return state;
    }
}

private class BunnyState(int a = 0, int b = 0, int c = 0, int d = 0)
{
    private const int TabSize = -8;

    public Dictionary<string, long> Mem { get; } =
        new()
        {
            ["a"] = a,
            ["b"] = b,
            ["c"] = c,
            ["d"] = d
        };
    
    private Dictionary<int, bool> TglState { get; } = new();
    
    public bool IsTgl(int index) => this.TglState.TryGetValue(index, out bool isTgl) && isTgl;
    
    public bool Tgl(int index) => this.TglState[index] = true;
    
    public override string ToString() =>
        new StringBuilder()
            .AppendLine($"{"a",TabSize}{"b",TabSize}{"c",TabSize}{"d",TabSize}")
            .AppendLine($"{Mem["a"],TabSize}{Mem["b"],TabSize}{Mem["c"],TabSize}{Mem["d"],TabSize}")
            .ToString();
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}