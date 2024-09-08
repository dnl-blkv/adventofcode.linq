<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int[] input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private enum OpCode
{
    Add = 1,
    Multiply = 2,
    Input = 3,
    Output = 4,
    JumpIfTrue = 5,
    JumpIfFalse = 6,
    LessThan = 7,
    Equals = 8,
    Halt = 99
}

private static readonly IReadOnlyDictionary<OpCode, int> ParamCounts =
    new Dictionary<OpCode, int>
    {
        [OpCode.Add] = 3,
        [OpCode.Multiply] = 3,
        [OpCode.Input] = 1,
        [OpCode.Output] = 1,
        [OpCode.JumpIfTrue] = 2,
        [OpCode.JumpIfFalse] = 2,
        [OpCode.LessThan] = 3,
        [OpCode.Equals] = 3,
        [OpCode.Halt] = 0
    };
    
private static readonly IReadOnlyList<int> UserInput1 = [1];
private static readonly IReadOnlyList<int> UserInput2 = [5];

private static long Solve1(int[] input) =>
    RunIntcodeProgram(inputProgram: input, userInput: UserInput1).Last();

private static long Solve2(int[] input) =>
    RunIntcodeProgram(inputProgram: input, userInput: UserInput2).Last();

private static IReadOnlyList<int> RunIntcodeProgram(int[] inputProgram, IEnumerable<int> userInput)
{   
    Span<int> program = inputProgram.ToArray().AsSpan<int>();
    var inputQueue = new Queue<int>(userInput);
    var output = new List<int>();
    int i = 0;
    
    while (true)
    {
        Command cmd = Command.Read(program, i);
        int continueFrom = i + cmd.Args.Count + 1;
        
        switch (cmd.OpCode)
        {
            case OpCode.Add:
                program[cmd.Args[2]] = cmd.GetArgVal(program, 0) + cmd.GetArgVal(program, 1);
                break;
                
            case OpCode.Multiply:
                program[cmd.Args[2]] = cmd.GetArgVal(program, 0) * cmd.GetArgVal(program, 1);
                break;
                
            case OpCode.Input:
                program[cmd.Args[0]] = inputQueue.Dequeue();
                break;
                
            case OpCode.Output:
                output.Add(program[cmd.Args[0]]);
                break;
                
            case OpCode.JumpIfTrue:
                if (cmd.GetArgVal(program, 0) > 0)
                {
                    continueFrom = cmd.GetArgVal(program, 1);
                }
            
                break;
                
            case OpCode.JumpIfFalse:
                if (cmd.GetArgVal(program, 0) is 0)
                {
                    continueFrom = cmd.GetArgVal(program, 1);
                }
            
                break;
                
            case OpCode.LessThan:
                program[cmd.Args[2]] = (cmd.GetArgVal(program, 0) < cmd.GetArgVal(program, 1) ? 1 : 0);
                break;
                
            case OpCode.Equals:
                program[cmd.Args[2]] = (cmd.GetArgVal(program, 0) == cmd.GetArgVal(program, 1) ? 1 : 0);
                break;
                
            case OpCode.Halt:
                return output;
                
            default:
                throw new InvalidOperationException($"Unexpected opCode: '{cmd.OpCode}'");
        }
        
        i = continueFrom;
    }
}

private static int[] ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',').Select(int.Parse).ToArray();
    
private record struct Command(OpCode OpCode, IReadOnlyList<int> Args, IReadOnlyList<int> ArgModes)
{
    public static Command Read (Span<int> program, int i)
    {
        int commandCode = program[i];
        
        int commandCodeRem = commandCode;
        OpCode opCode = (OpCode)(commandCodeRem % 100);
        commandCodeRem /= 100;
    
        int argsStart = i + 1;
        int argsCount = ParamCounts[opCode];
        int[] args = program[argsStart..(argsStart + argsCount)].ToArray();
        var argModes = new int[args.Length];
    
        for (int p = 0; p < args.Length; p++)
        {
            argModes[p] = commandCodeRem % 10;
            commandCodeRem /= 10;
        }
        
        return new Command(OpCode: opCode, Args: args, ArgModes: argModes);
    }
    
    public int GetArgVal(Span<int> program, int index)
    {
        int immediateVal = this.Args[index];
        
        return (this.ArgModes[index] is 1 ? immediateVal : program[immediateVal]);
    }
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
