<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<long> input = ParseInput(GetInput());
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
    RelativeBaseOffset = 9,
    Halt = 99
}

private const int ExtraTapeLength = 1238;

private const char NewLine = '\n';
private const char CellEmpty = '.';
private const char CellScaffold = '#';

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
    
private static readonly IReadOnlyList<Point> Directions =
[
    new Point(I:  0, J:  1),
    new Point(I:  1, J:  0),
    new Point(I:  0, J: -1),
    new Point(I: -1, J:  0),
];

private static long Solve1(IReadOnlyList<long> input)
{
    IReadOnlyList<long> output = Program.Create(input.Concat(Enumerable.Repeat(0L, ExtraTapeLength))).Run([]);
    List<List<char>> map = BuildMap(output);
    
    return
        Enumerable.Range(1, map.Count - 2)
            .Sum(i => Enumerable.Range(1, map[i].Count - 2)
            .Sum(j =>
                Directions.All(d => map[i + d.I][j + d.J] is CellScaffold)
                    ? i * j
                    : 0));
}

private static long Solve2(IReadOnlyList<long> input)
{
    List<long> fullInput = input.Concat(Enumerable.Repeat(0L, ExtraTapeLength)).ToList();
    fullInput[0] = 2;
    IReadOnlyList<long> output =
        Program.Create(fullInput).Run(
            Enumerable.Empty<char>()
                .Concat("A,B,A,B,C,C,B,A,B,C\n")
                .Concat("L,4,R,8,L,6,L,10\n")
                .Concat("L,6,R,8,R,10,L,6,L,6\n")
                .Concat("L,4,L,4,L,10\n")
                .Concat("n\n")
                .Select(c => (long)c)
                .ToList());
    
    /* The program is written manually based on the output from:
     *
     * List<List<char>> map = BuildMap(output);
     * map.Select(l => string.Join(string.Empty, l)).Dump();
     */
    
    return output.Last();
}

private static List<List<char>> BuildMap(IReadOnlyList<long> programOutput)
{
    List<List<char>> map = [[]];
    
    foreach (char c in programOutput.Select(n => (char)n))
    {
        if (c is NewLine)
        {
            map.Add([]);
            continue;
        }
        
        map[^1].Add(c);
    }
    
    int mapWidth = map.Max(l => l.Count);
    
    foreach (List<char> line in map)
    {
        PadRight(line, mapWidth, value: CellEmpty);
    }
    
    return map;
}

private static void PadRight<T>(List<T> list, int totalCount, T value)
{
    for (int i = list.Count; i < totalCount; i++)
    {
        list.Add(value);
    }
}

private static IReadOnlyList<long> ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',').Select(long.Parse).ToList();
    
private readonly record struct Point(int I, int J)
{
    public static Point operator+(Point a, Point b) =>
        new Point(I: a.I + b.I, J: a.J + b.J);
}
    
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
