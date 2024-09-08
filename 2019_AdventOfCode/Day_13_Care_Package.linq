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

private enum TileType : long
{
    Empty = 0,
    Wall = 1,
    Block = 2,
    Paddle = 3,
    Ball = 4
}

private const int ExtraTapeLength = 14;

private const int OutputBlockSize = 3;
private const int QuarterInsertedCount = 2;
private const int InBlockXPosIndex = 0;
private const int InBlockYPosIndex = 1;
private const int InBlockTileTypeIndex = 2;
private const int DebugBlockCount = 0; // Change this number to N to print the game screen for the first N blocks

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

private static readonly IReadOnlyDictionary<TileType, char> TileChars =
    new Dictionary<TileType, char>
    {
        [TileType.Empty] = '\'',
        [TileType.Wall] = '#',
        [TileType.Block] = '+',
        [TileType.Paddle] = 'T',
        [TileType.Ball] = '@'
    };

private static (long Result1, long Result2) Solve(IReadOnlyList<long> input)
{
    long[] programCode = input.Concat(Enumerable.Repeat(0L, ExtraTapeLength)).ToArray();
    programCode[0] = QuarterInsertedCount;
    
    var program = Program.Create(programCode);
    List<long> debugOutput = [];
    HashSet<Point> blocks = [];
    int ballX = -1;
    int paddleX = -1;
    int result1 = 0;
    long result2 = 0;
    
    do
    {
        IReadOnlyList<long> newOutput = program.Run(GetNextInput());
        PrintGameScreen(newOutput);
        
        for (int i = 0; i < newOutput.Count; i += OutputBlockSize)
        {
            int x = (int)newOutput[i + InBlockXPosIndex];
            long tileTypeLong = newOutput[i + InBlockTileTypeIndex];
        
            if (x < 0)
            {
                result2 = tileTypeLong;
                continue;
            }
            
            int y = (int)newOutput[i + InBlockYPosIndex];
            
            switch ((TileType)tileTypeLong)
            {
                case TileType.Empty:
                    result1 += (blocks.Remove(new Point(Y: y, X: x)) ? 1 : 0);
                    break;
                    
                case TileType.Wall:
                    break;
                    
                case TileType.Block:
                    blocks.Add(new Point(Y: y, X: x));
                    break;
                    
                case TileType.Paddle:
                    paddleX = x;
                    break;
                    
                case TileType.Ball:
                    ballX = x;
                    break;
                    
                default:
                    throw new InvalidOperationException($"Unknown tile type long: {tileTypeLong}.");
            }
        }
    } while (blocks.Count > 0);
    
    return (Result1: result1, Result2: result2);
    
    IEnumerable<long> GetNextInput()
    {
        if (ballX < 0)
        {
            yield break;
        }
    
        int ballPaddleDelta = ballX - paddleX;
        
        yield return
            ballPaddleDelta is 0
                ? 0
                : ballPaddleDelta / Math.Abs(ballPaddleDelta);
    }
    
    void PrintGameScreen(IEnumerable<long> newOutput)
    {
        if (result1 >= DebugBlockCount)
        {
            return;
        }
        
        debugOutput.AddRange(newOutput);
        RenderScreen(debugOutput).Dump();
    }
}

private static string RenderScreen(IReadOnlyList<long> output)
{
    long maxY = long.MinValue;
    long maxX = long.MinValue;
    
    for (int i = 0; i < output.Count; i += OutputBlockSize)
    {
        maxY = Math.Max(maxY, output[i + InBlockYPosIndex]);
        maxX = Math.Max(maxX, output[i + InBlockXPosIndex]);
    }
    
    int screenHeight = (int)maxY + 1;
    int screenWidth = (int)maxX + 1;
    
    char[][] screen =
        Enumerable.Range(0, screenHeight)
            .Select(_ => Enumerable.Repeat(TileChars[TileType.Empty], screenWidth).ToArray())
            .ToArray();
            
    string scoreLine = string.Empty;
    
    for (int i = 0; i < output.Count; i += OutputBlockSize)
    {
        int y = (int)output[i + InBlockYPosIndex];
        int x = (int)output[i + InBlockXPosIndex];
        long tileTypeLong = output[i + InBlockTileTypeIndex];
        
        if (x < 0)
        {
            scoreLine = $"Score: {tileTypeLong}";
            continue;
        }
        
        screen[y][x] = TileChars[(TileType)tileTypeLong];
    }
    
    var renderedScreenBuilder = new StringBuilder();
    renderedScreenBuilder.AppendLine(scoreLine);
    
    return screen
        .Aggregate(renderedScreenBuilder, (sb, l) => sb.AppendLine(string.Join(string.Empty, l)))
        .ToString();
}

private static IReadOnlyList<long> ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',').Select(long.Parse).ToArray();
    
private record struct Point(int Y, int X);

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
}
    
private record struct Command(
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
