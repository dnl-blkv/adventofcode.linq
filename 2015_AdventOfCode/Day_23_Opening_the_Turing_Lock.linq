<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<string> program = ParseInput(GetInput());
	Solve1(program).Dump();
	Solve2(program).Dump();
}

private delegate Func<int> CommandFactory(
    IReadOnlyList<string> program,
    Dictionary<string, int> memory,
    string posRegisterName);

private static readonly IReadOnlyDictionary<string, CommandFactory> DefaultCommandFactories =
    new Dictionary<string, CommandFactory>
    {
        ["hlf"] = (args, memory, posRegisterName) =>
            () =>
            {
                memory[args[0]] /= 2;
                return memory[posRegisterName] + 1;
            },
            
        ["tpl"] = (args, memory, posRegisterName) =>
            () =>
            {
                memory[args[0]] *= 3;
                return memory[posRegisterName] + 1;
            },
        
        ["inc"] = (args, memory, posRegisterName) =>
            () =>
            {
                memory[args[0]]++;
                return memory[posRegisterName] + 1;
            },
            
        ["jmp"] = (args, memory, posRegisterName) =>
        {
            int jumpValue = int.Parse(args[0]);
            return () => memory[posRegisterName] + jumpValue;
        },
        
        ["jie"] = (args, memory, posRegisterName) =>
        {
            int jumpValue = int.Parse(args[1]);
            return () => memory[posRegisterName] + (memory[args[0]] % 2 == 0 ? jumpValue : 1);
        },
        
        ["jio"] = (args, memory, posRegisterName) =>
        {
            int jumpValue = int.Parse(args[1]);
            return () => memory[posRegisterName] + (memory[args[0]] == 1 ? jumpValue : 1);
        },
    };

private static int Solve1(IEnumerable<string> program) => Solve(program, initialA: 0);

private static int Solve2(IEnumerable<string> program) => Solve(program, initialA: 1);

private static int Solve(IEnumerable<string> program, int initialA)
{   
    const string A = "a";
    const string B = "b";
    const string Pos = "pos";
    var memory = new Dictionary<string, int>
    {
        [A] = initialA,
        [B] = 0,
        [Pos] = 0
    };
    Func<int>[] programCompiled = CompileProgram(program, memory, DefaultCommandFactories, Pos);
    
    while (0 <= memory[Pos] && memory[Pos] < programCompiled.Length)
    {
        memory[Pos] = programCompiled[memory[Pos]].Invoke();
    }
    
    return memory[B];
}

private static Func<int>[] CompileProgram(
    IEnumerable<string> program,
    Dictionary<string, int> memory,
    IReadOnlyDictionary<string, CommandFactory> commandFactories,
    string posRegisterName) =>
    program
        .Select(command =>
        {
            string[] commandParts = command.Split(" ", 2);
            return commandFactories[commandParts[0]].Invoke(commandParts[1].Split(", "), memory, posRegisterName);
        })
        .ToArray();

private static IReadOnlyList<string> ParseInput(IEnumerable<string> input) => input.ToArray();
        
private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}