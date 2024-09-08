<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<long> input = ParseInput(GetInput());
    Solve(input).Dump();
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
    RelativeBaseOffset = 9,
    Halt = 99
}

private const int ExtraTapeLength = 67;

private const string ExecutionType1 = "WALK";
private const string ExecutionType2 = "RUN";

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
        [OpCode.RelativeBaseOffset] = 1,
        [OpCode.Halt] = 0
    };

private static (long Result1, long Result2) Solve(IReadOnlyList<long> input)
{
    IReadOnlyList<long> extendedInput = input.Concat(Enumerable.Repeat(0L, ExtraTapeLength)).ToList();
    
    IReadOnlyList<char> routine1 =
         Enumerable.Empty<char>()
            .Concat("NOT A T\n")
            .Concat("OR T J\n")
            .Concat("NOT B T\n")
            .Concat("OR T J\n")
            .Concat("NOT C T\n")
            .Concat("OR T J\n")
            .Concat("AND D J\n")
            .ToList();
    IReadOnlyList<long> output1 = ExecuteSpringScript(routine: routine1, executionType: ExecutionType1);
            
    // To debug routine1, uncomment the following line:
    // ToDebugString(output1).Dump();
    
    IEnumerable<char> routine2 =
        routine1
            .Concat("NOT D T\n")
            .Concat("OR I T\n")
            .Concat("OR F T\n")
            .Concat("AND E T\n")
            .Concat("OR H T\n")
            .Concat("AND T J\n");
    IReadOnlyList<long> output2 = ExecuteSpringScript(routine: routine2, executionType: ExecutionType2);
            
    // To debug routine2, uncomment the following line:
    // ToDebugString(output2).Dump();
    
    return (Result1: output1[^1], Result2: output2[^1]);
    
    IReadOnlyList<long> ExecuteSpringScript(IEnumerable<char> routine, string executionType) =>
        Program.Create(extendedInput).Run(routine.Concat($"{executionType}\n").Select(c => (long)c));
        
    string ToDebugString(IEnumerable<long> output) =>
        string.Join(string.Empty, output.Select(c => (char)c));
}

private static IReadOnlyList<long> ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',').Select(long.Parse).ToList();
    
private class Program
{
    private readonly long[] code;
    private int ins;
    private long relativeBase;

    private Program(long[] code)
        : this(code, ins: 0, relativeBase: 0, isHalted: false) => Expression.Empty();
    
    private Program(long[] code, int ins, long relativeBase, bool isHalted)
    {
        this.code = code;
        this.ins = ins;
        this.relativeBase = relativeBase;
        this.IsHalted = isHalted;
    }
    
    public bool IsHalted { get; private set; }
    
    public static Program Create(IEnumerable<long> code) => new Program(code.ToArray());
    
    public IReadOnlyList<long> Run(IEnumerable<long> userInput)
    {
        if (this.IsHalted)
        {
            throw new InvalidOperationException("Can't run: the program has halted before.");
        }
    
        var outputStream = new List<long>();
        var inputQueue = new Queue<long>(userInput);
        
        while (true)
        {
            Command cmd = Command.Read(this.code, i: ins, relativeBase: this.relativeBase);
            int continueFrom = this.ins + cmd.Args.Count + 1;
            
            switch (cmd.OpCode)
            {
                case OpCode.Add:
                    this.code[cmd.GetPositionArgVal(this.code, 2)] =
                        cmd.GetArgVal(this.code, 0) + cmd.GetArgVal(this.code, 1);
                    break;
                    
                case OpCode.Multiply:
                    this.code[cmd.GetPositionArgVal(this.code, 2)] =
                        cmd.GetArgVal(this.code, 0) * cmd.GetArgVal(this.code, 1);
                    break;
                    
                case OpCode.Input:
                    if (inputQueue.Count is 0)
                    {
                        return outputStream;
                    }
                    
                    this.code[cmd.GetPositionArgVal(this.code, 0)] = inputQueue.Dequeue();
                    break;
                    
                case OpCode.Output:
                    outputStream.Add(cmd.GetArgVal(this.code, 0));
                    break;
                    
                case OpCode.JumpIfTrue:
                    if (cmd.GetArgVal(this.code, 0) > 0)
                    {
                        continueFrom = (int)cmd.GetArgVal(this.code, 1);
                    }
                
                    break;
                    
                case OpCode.JumpIfFalse:
                    if (cmd.GetArgVal(this.code, 0) is 0)
                    {
                        continueFrom = (int)cmd.GetArgVal(this.code, 1);
                    }
                
                    break;
                    
                case OpCode.LessThan:
                    this.code[cmd.GetPositionArgVal(this.code, 2)] =
                        (cmd.GetArgVal(this.code, 0) < cmd.GetArgVal(this.code, 1) ? 1 : 0);
                    break;
                    
                case OpCode.Equals:
                    this.code[cmd.GetPositionArgVal(this.code, 2)] =
                        (cmd.GetArgVal(this.code, 0) == cmd.GetArgVal(this.code, 1) ? 1 : 0);
                    break;
                    
                case OpCode.RelativeBaseOffset:
                    this.relativeBase += cmd.GetArgVal(this.code, 0);
                    break;
                    
                case OpCode.Halt:
                    this.IsHalted = true;
                    return outputStream;
                    
                default:
                    throw new InvalidOperationException($"Unexpected opCode: '{cmd.OpCode}'");
            }
            
            this.ins = continueFrom;
        }
    }
    
    public Program Clone() =>
        new Program(
            code: this.code.ToArray(),
            ins: this.ins,
            relativeBase: this.relativeBase,
            isHalted: this.IsHalted);
}
    
private readonly record struct Command(
    OpCode OpCode,
    IReadOnlyList<long> Args,
    IReadOnlyList<long> ArgModes,
    long RelativeBase)
{
    public static Command Read (ReadOnlySpan<long> code, int i, long relativeBase)
    {
        long commandCode = code[i];
        
        long commandCodeRem = commandCode;
        OpCode opCode = (OpCode)(commandCodeRem % 100);
        commandCodeRem /= 100;
    
        int argsStart = i + 1;
        int argsCount = ParamCounts[opCode];
        long[] args = code[argsStart..(argsStart + argsCount)].ToArray();
        var argModes = new long[args.Length];
    
        for (long p = 0; p < args.Length; p++)
        {
            argModes[p] = commandCodeRem % 10;
            commandCodeRem /= 10;
        }
        
        return new Command(
            OpCode: opCode,
            Args: args,
            ArgModes: argModes,
            RelativeBase: relativeBase);
    }
    
    public long GetArgVal(IReadOnlyList<long> code, int index)
    {
        long argMode = this.ArgModes[index];
        
        return argMode switch
        {
            1 => this.Args[index],
            0 or 2 => code[(int)this.GetPositionArgVal(code, index)],
            _ => throw new InvalidOperationException($"Unexpected read argMode: '{argMode}'")
        };
    }
    
    public long GetPositionArgVal(IReadOnlyList<long> code, int index)
    {
        long immediateVal = this.Args[index];
        long argMode = this.ArgModes[index];
        
        return argMode switch
        {
            0 => (int)immediateVal,
            2 => (int)(this.RelativeBase + immediateVal),
            _ => throw new InvalidOperationException($"Unexpected write argMode: '{argMode}'")
        };
    }
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
