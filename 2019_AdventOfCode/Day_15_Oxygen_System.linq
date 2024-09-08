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

private const long MoveResultCodeWallHit = 0;
private const long MoveResultCodeOxygenSystemFound = 2;

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
    
private static readonly IReadOnlyDictionary<Point, long> DirectionCodes =
    new Dictionary<Point, long>()
    {
        [new Point(Y: -1, X:  0)] = 1,
        [new Point(Y:  1, X:  0)] = 2,
        [new Point(Y:  0, X:  1)] = 3,
        [new Point(Y:  0, X: -1)] = 4
    };

private static (long Result1, long Result2) Solve(IReadOnlyList<long> input)
{
    (int result1, Point initialPosition, HashSet<Point> visited) = SearchForOxygenSystem(input);
    visited.Add(initialPosition);
    IReadOnlyList<Point> currentStates = [initialPosition];
    int result2 = 0;

    while (true)
    {
        currentStates =
            currentStates
                .SelectMany(s => DirectionCodes.Keys.Select(d => s + d).Where(visited.Add))
                .ToArray();

        if (currentStates.Count is 0)
        {
            return (Result1: result1, Result2: result2);
        }

        result2++;
    }
}

private static (
    int StepsToOxygenSystem,
    Point OxygenSystemPosition,
    HashSet<Point> WallCells) SearchForOxygenSystem(IReadOnlyList<long> input)
{
    var initialState = new State(
        Position: new Point(Y: 0, X: 0),
        Program: Program.Create(input));
    IReadOnlyList<State> currentStates = [initialState];
    var visited = new Dictionary<Point, bool>
    {
        [initialState.Position] = false
    };
    int stepsToOxygenSystem = -1;
    var oxygenSystemPosition = new Point(Y: int.MinValue, X: int.MinValue);
    int stepsSoFar = 0;
    
    while (currentStates.Count > 0)
    {
        List<State> nextStates = [];
        
        foreach (State currentState in currentStates)
        {
            foreach (Point directionDelta in DirectionCodes.Keys)
            {
                Point nextPosition = currentState.Position + directionDelta;
                
                if (visited.ContainsKey(nextPosition))
                {
                    continue;
                }
                
                Program nextProgram = currentState.Program.Clone();
                long directionCode = DirectionCodes[directionDelta];
                long moveResultCode = nextProgram.Run([directionCode]).Single();
                
                if (moveResultCode is MoveResultCodeWallHit)
                {
                    visited[nextPosition] = true;
                    continue;
                }
                
                if (moveResultCode is MoveResultCodeOxygenSystemFound)
                {
                    stepsToOxygenSystem = stepsSoFar + 1;
                    oxygenSystemPosition = nextPosition;
                }
                
                visited[nextPosition] = false;
                nextStates.Add(new State(Position: nextPosition, Program: nextProgram));
            }
        }
        
        currentStates = nextStates;
        stepsSoFar++;
    }
           
    return (
        StepsToOxygenSystem: stepsToOxygenSystem,
        OxygenSystemPosition: oxygenSystemPosition,
        WallCells: visited.Where(kv => kv.Value).Select(kv => kv.Key).ToHashSet());
}

private static IReadOnlyList<long> ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',').Select(long.Parse).ToArray();
    
private record struct State(Point Position, Program Program);
    
private record struct Point(int Y, int X)
{
    public static Point operator+(Point a, Point b) =>
        new Point(Y: a.Y + b.Y, X: a.X + b.X);
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
