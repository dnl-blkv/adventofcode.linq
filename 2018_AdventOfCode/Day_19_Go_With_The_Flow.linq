<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (int ip, IReadOnlyList<(string Cmd, int[] Args)> program) = ParseInput(GetInput());
    // Uncomment the line below to see unoptimized C# code of the input program:
    // Visualize(ip, program);
    Solve1(ip, program).Dump();
    Solve2(ip, program).Dump();
}

private delegate string Visualizer(int a, int b, int c);
private delegate void Operation(int[] reg, int a, int b, int c);

private const int StartingRegZero1 = 0;
private const int StartingRegZero2 = 1;

private static readonly IReadOnlyDictionary<string, Visualizer> Visualizers =
    new Dictionary<string, Visualizer>
    {
        ["addr"] = (a, b, c) => $"p{c} = p{a} + p{b};",
        ["addi"] = (a, b, c) => $"p{c} = p{a} + {b};",
        ["mulr"] = (a, b, c) => $"p{c} = p{a} * p{b};",
        ["muli"] = (a, b, c) => $"p{c} = p{a} * {b};",
        ["banr"] = (a, b, c) => $"p{c} = p{a} & p{b};",
        ["bani"] = (a, b, c) => $"p{c} = p{a} & {b};",
        ["borr"] = (a, b, c) => $"p{c} = p{a} | p{b};",
        ["bori"] = (a, b, c) => $"p{c} = p{a} | {b};",
        ["setr"] = (a, b, c) => $"p{c} = p{a};",
        ["seti"] = (a, b, c) => $"p{c} = {a};",
        ["gtir"] = (a, b, c) => $"p{c} = ({a} > p{b} ? 1 : 0);",
        ["gtri"] = (a, b, c) => $"p{c} = (p{a} > {b} ? 1 : 0);",
        ["gtrr"] = (a, b, c) => $"p{c} = (p{a} > p{b} ? 1 : 0);",
        ["eqir"] = (a, b, c) => $"p{c} = ({a} == p{b} ? 1 : 0);",
        ["eqri"] = (a, b, c) => $"p{c} = (p{a} == {b} ? 1 : 0);",
        ["eqrr"] = (a, b, c) => $"p{c} = (p{a} == p{b} ? 1 : 0);"
    };

private static readonly IReadOnlyDictionary<string, Operation> Operations =
    new Dictionary<string, Operation>
    {
        ["addr"] = (reg, a, b, c) => reg[c] = reg[a] + reg[b],
        ["addi"] = (reg, a, b, c) => reg[c] = reg[a] + b,
        ["mulr"] = (reg, a, b, c) => reg[c] = reg[a] * reg[b],
        ["muli"] = (reg, a, b, c) => reg[c] = reg[a] * b,
        ["banr"] = (reg, a, b, c) => reg[c] = reg[a] & reg[b],
        ["bani"] = (reg, a, b, c) => reg[c] = reg[a] & b,
        ["borr"] = (reg, a, b, c) => reg[c] = reg[a] | reg[b],
        ["bori"] = (reg, a, b, c) => reg[c] = reg[a] | b,
        ["setr"] = (reg, a, b, c) => reg[c] = reg[a],
        ["seti"] = (reg, a, b, c) => reg[c] = a,
        ["gtir"] = (reg, a, b, c) => reg[c] = (a > reg[b] ? 1 : 0),
        ["gtri"] = (reg, a, b, c) => reg[c] = (reg[a] > b ? 1 : 0),
        ["gtrr"] = (reg, a, b, c) => reg[c] = (reg[a] > reg[b] ? 1 : 0),
        ["eqir"] = (reg, a, b, c) => reg[c] = (a == reg[b] ? 1 : 0),
        ["eqri"] = (reg, a, b, c) => reg[c] = (reg[a] == b ? 1 : 0),
        ["eqrr"] = (reg, a, b, c) => reg[c] = (reg[a] == reg[b] ? 1 : 0)
    };
    
private static long Solve1(int ip, IReadOnlyList<(string Cmd, int[] Args)> program) =>
    Solve(ip, program, StartingRegZero1);
    
private static long Solve2(int ip, IReadOnlyList<(string Cmd, int[] Args)> program) =>
    Solve(ip, program, StartingRegZero2);

private static long Solve(int ip, IReadOnlyList<(string Cmd, int[] Args)> program, int startingRegZero)
{
    int[] reg = [startingRegZero, 0, 0, 0, 0, 0];
    
    while (reg[ip] != 4)
    {
        (string cmd, int[] args) = program[reg[ip]];
        Operations[cmd](reg, args[0], args[1], args[2]);
        reg[ip]++;
    }
    
    // Yes, this is it, it was:
    // """
    // Set reg[1] (logic starting at line 17), then find sum of all the divisors
    // of its value, including the value itself. Is that all it takes to hack time?
    // """
    return Enumerable.Range(1, (int)reg[1]).Sum(k => (reg[1] % k is 0 ? k : 0));
}

private static void Visualize(int ip, IReadOnlyList<(string Cmd, int[] Args)> program)
{
    const string HLine = "--------------------";
    "Program to optimize: ".Dump();
    HLine.Dump();

    foreach (((string cmd, int[] args), int i) in program.Select((l, i) => (l, i)))
    {
        string code = Visualizers[cmd].Invoke(args[0], args[1], args[2]);
        int indexOfAssignment = code.IndexOf('=');
        string left = code[..indexOfAssignment].Replace($"p{ip}", "ins");
        string right = code[indexOfAssignment..].Replace($"p{ip}", i.ToString());
        $"{i}: {left}{right}".Dump();
    }
    
    HLine.Dump();
    string.Empty.Dump();
}

private static (int Ip, IReadOnlyList<(string Cmd, int[] Args)> Program) ParseInput(IEnumerable<string> input)
{
    int ip = -1;
    List<(string Cmd, int[] Args)> program = [];
    
    foreach ((string line, int i) in input.Select((l, i) => (l, i)))
    {
        if (i == 0)
        {
            ip = line[^1] - '0';
            continue;
        }
        
        program.Add(ParseProgramLine(line));
    }

    return (Ip: ip, Program: program);
}

private static (string Cmd, int[] Args) ParseProgramLine(string line)
{
    string[] lineParts = line.Split(' ');
    
    return (Cmd: lineParts[0], Args: ToIntArray(lineParts.Skip(1)));
}
        
private static int[] ToIntArray(IEnumerable<string> intStrings) =>
    intStrings.Select(int.Parse).ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
