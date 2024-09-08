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

private const int MinAddress = 0;
private const int ComputerCount = 50;
private const int NatAddress = 255;

private const int ExtraTapeLength = 9;

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
    var nat = new NAT();
    IReadOnlyList<Computer> computers = CreateComputers(input);
    var packetQueue = new Queue<Packet>(computers.SelectMany(c => c.Receive()));
    (long Result1, long Result2) results;
    
    do
    {
        while (packetQueue.Count > 0)
        {
            Run(packetQueue.Dequeue());
        }
    
        Run(nat.Memory);
    } while (!nat.TryStopProcessing(out results));
    
    return results;
    
    void Run(Packet packet)
    {
        if (packet.Destination is NatAddress)
        {
            nat.SetMemory(packet);
            return;
        }
    
        foreach (Packet newPacket in computers[packet.Destination].Receive(packet))
        {
            packetQueue.Enqueue(newPacket);
        }
    }
}

private static IReadOnlyList<Computer> CreateComputers(IReadOnlyList<long> input)
{
    IReadOnlyList<long> extendedInput = input.Concat(Enumerable.Repeat(0L, ExtraTapeLength)).ToList();

    return Enumerable.Range(MinAddress, ComputerCount)
        .Select(i => Computer.Create(extendedInput, i))
        .ToList();
}

private static IReadOnlyList<long> ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',').Select(long.Parse).ToList();
    
private class NAT
{
    private Packet? memory = null;
    private Packet? firstPacketReceived = null;
    private Packet? lastPacketSent = null;
    
    public Packet Memory => this.memory ?? throw new InvalidOperationException("Memory is not initialized.");
        
    public void SetMemory(Packet natPacket)
    {
        this.memory = natPacket with { Destination = MinAddress };
        this.firstPacketReceived ??= this.memory;
    }
    
    public bool TryStopProcessing(out (long Result1, long Result2) results)
    {
        if (this.Memory.Y != this.lastPacketSent?.Y)
        {
            results = default;
            this.lastPacketSent = this.Memory;
            return false;
        }
        
        results = (
            Result1: this.firstPacketReceived!.Value.Y,
            Result2: this.lastPacketSent!.Value.Y);
        return true;
    }
}
    
private class Computer(Program program)
{
    private const long NoInput = -1;

    private const int PacketLengthInOutput = 3;
    private const int DestinationOffset = 0;
    private const int XOffset = 1;
    private const int YOffset = 2;

    public static Computer Create(IEnumerable<long> code, int address)
    {
        _ = address < 0
            ? throw new ArgumentOutOfRangeException(
                message: $"Address must be positive.",
                paramName: nameof(address))
            : address;
    
        var program = Program.Create(code);
        program.Run([address]);
        
        return new Computer(program);
    }
    
    public IEnumerable<Packet> Receive() =>
        ParsePackets(program.Run([NoInput]));
    
    public IEnumerable<Packet> Receive(Packet packet) =>
        ParsePackets(program.Run([packet.X, packet.Y]));
    
    private static IEnumerable<Packet> ParsePackets(IReadOnlyList<long> output)
    {   
        for (int i = 0; i < output.Count; i += PacketLengthInOutput)
        {
            int destination = (int)output[i + DestinationOffset];
            long x = output[i + XOffset];
            long y = output[i + YOffset];
            yield return new Packet(Destination: destination, X: x, Y: y);
        }
    }
}

private readonly record struct Packet(int Destination, long X, long Y);
    
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
