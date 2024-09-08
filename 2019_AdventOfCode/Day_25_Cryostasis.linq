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

private const int ExtraTapeLength = 500;

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
    
// All the available locations, mapped manually, text-based RPG are FUN
private static readonly IReadOnlyDictionary<string, string> Locations =
    new Dictionary<string, string>
    {
        [""] = "Hull Breach",
        ["east"] = "Gift Wrapping Center", // sand
        ["east,east"] = "Kitchen", // molten lava
        ["south"] = "Crew Quarters", // ornament
        ["south,east"] = "Observatory",
        ["west"] = "Sick Bay",
        ["west,north"] = "Hallway", // wreath
        ["west,north,east"] = "Engineering", // fixed point
        ["west,north,north"] = "Corridor", // infinite loop
        ["west,north,north,north"] = "Passages", // spool of cat6
        ["west,south"] = "Arcade", // giant electromagnet
        ["west,south,south"] = "Holodeck", // candy cane
        ["west,south,east"] = "Warp Drive Maintenance", // escape pod
        ["west,south,east,south"] = "Stables",
        ["west,south,east,east"] = "Navigation",
        ["west,south,east,east,south"] = "Hot Chocolate Fountain", // photons
        ["west,south,east,east,east"] = "Storage", // space law space brochure
        ["west,south,east,east,east,south"] = "Science Lab", // fuel cell
        ["west,south,east,east,east,south,south"] = "Security Checkpoint",
        ["west,south,east,east,east,south,south,west"] = "Pressure-Sensitive Floor"
    };

private static long Solve(IReadOnlyList<long> input)
{
    Program program = Program.Create(input.Concat(Enumerable.Repeat(0L, ExtraTapeLength)));
    
    // Thought process:
    // 1. Walk the shortest path and pick everything that doesn't disable you. The ones that disable you are:
    //     1. "lava" (it melts you)
    //     2. "infinite loop" (it melts your PC)
    //     3. "giant electromagnet" (it gets stuck to you and you can't move)
    //     4. "escape pod" (it launches you into space)
    //     5. "photons" (it gets dark and you are eaten by a Grue)
    // 2. Get to the "Security Checkpoint"
    // 3. Start trying to step on "Pressure-Sensitive Floor" and experiment with dropping items right before it
    // 4. It quickly becomes apparent that:
    //     1. Both "spool of cat6" and "ornament" are each too heavy alone, so they are not needed
    //     2. "fuel cell" and "wreath" are too heavy together, so we may need at most one of them
    //     3. "fuel cell" + everything not dismissed in (4.1) and (4.2) are too light all together,
    //        so we _need_ "wreath" + some of the other four items, but not the "fuel cell"
    //     4. "wreath" + the other four items are too heavy together, so we need to take at least one item away
    //     5. Start by trying different sets of three out of the four remaining items, and we'll find the answer
    IReadOnlyList<long> output =
        program.Run(
            Enumerable.Empty<char>()
                .Concat("east\n")
                .Concat("take sand\n")
                .Concat("west\n")
                .Concat("south\n")
                .Concat("take ornament\n")
                .Concat("north\n")
                .Concat("west\n")
                .Concat("north\n")
                .Concat("take wreath\n")
                .Concat("east\n")
                .Concat("take fixed point\n")
                .Concat("west\n")
                .Concat("north\n")
                .Concat("north\n")
                .Concat("take spool of cat6\n")
                .Concat("south\n")
                .Concat("south\n")
                .Concat("south\n")
                .Concat("south\n")
                .Concat("south\n")
                .Concat("take candy cane\n")
                .Concat("north\n")
                .Concat("east\n")
                .Concat("east\n")
                .Concat("east\n")
                .Concat("take space law space brochure\n")
                .Concat("south\n")
                .Concat("take fuel cell\n")
                .Concat("south\n")
                
                // Too Heavy
                .Concat("drop spool of cat6\n")
                .Concat("drop ornament\n")
                
                // Too light (too much with wreath alone, too little with everything else that is not "Too Heavy")
                .Concat("drop fuel cell\n")
                
                // Not needed, the other four solve it
                .Concat("drop candy cane\n")
                
                // These are the items we end up keeping (by not dropping them, that is):
                //.Concat("drop wreath\n")
                //.Concat("drop sand\n")
                //.Concat("drop fixed point\n")
                //.Concat("drop space law space brochure\n")
                
                .Concat("west\n")
                .Select(c => (long)c)
                .ToList());
                
    ToDebugString(output).Dump();            
            
    return -1;
    
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
