<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<int> input = ParseInput(GetInput());
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

private const int AmplifierCount = 5;
private const int MinPhaseSetting1 = 0;
private const int MinPhaseSetting2 = 5;
private const int InitialInputSignal = 0;

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

private static long Solve1(IReadOnlyList<int> input)
{
    IReadOnlyList<int> availablePhaseSettings = Enumerable.Range(MinPhaseSetting1, AmplifierCount).ToArray();
    int maxOutputSignal = int.MinValue;

    foreach (IEnumerable<int> phaseSettings in Permute(availablePhaseSettings))
    {
        int lastOutputSignal = InitialInputSignal;
        
        foreach ((int phaseSetting, int i) in phaseSettings.Select((p, i) => (p, i)))
        {
            IReadOnlyList<int> userInput = [phaseSetting, lastOutputSignal];
            Program program = Program.Create(input);
            lastOutputSignal = program.Run(userInput).Last();
            maxOutputSignal = Math.Max(maxOutputSignal, lastOutputSignal);
        }
    }
    
    return maxOutputSignal;
}

private static long Solve2(IReadOnlyList<int> input)
{
    IReadOnlyList<int> availablePhaseSettings = Enumerable.Range(MinPhaseSetting2, AmplifierCount).ToArray();
    int maxOutputSignal = int.MinValue;

    foreach (int[] phaseSettings in Permute(availablePhaseSettings).Select(ps => ps.ToArray()))
    {
        Program[] programs = phaseSettings.Select(_ => Program.Create(input)).ToArray();
        int i = 0;
        int p = 0;
        int lastOutputSignal = InitialInputSignal;
        
        do
        {
            bool firstRun = i < AmplifierCount;
            var userInput = new List<int>(firstRun ? 2 : 1);
            
            if (firstRun)
            {
                userInput.Add(phaseSettings[p]);
            }
            
            userInput.Add(lastOutputSignal);
            
            lastOutputSignal = programs[p].Run(userInput).Last();
        } while (!programs[p = ++i % AmplifierCount].IsHalted);
        
        maxOutputSignal = Math.Max(maxOutputSignal, lastOutputSignal);
    }
    
    return maxOutputSignal;
}

private static IEnumerable<IEnumerable<int>> Permute(IReadOnlyList<int> list, int nextIndex = 0)
{
    int rangeLength = list.Count - nextIndex;

    if (rangeLength is 0)
    {
        yield return Enumerable.Empty<int>();
        yield break;
    }

    if (rangeLength is 1)
    {
        yield return Enumerable.Empty<int>().Append(list[nextIndex]);
        yield break;
    }
    
    for (int i = 0; i <= rangeLength - 1; i++)
    {
        foreach (IEnumerable<int> subPermutation in Permute(list, nextIndex + 1))
        {
            yield return InsertAt(subPermutation, position: i, newValue: list[nextIndex]);
        }
    }
}

private static IEnumerable<int> InsertAt(IEnumerable<int> enumerable, int position, int newValue)
{
    if (position is 0)
    {
        yield return newValue;
    }
    
    int i = 0;

    foreach (int value in enumerable)
    {
        yield return value;
    
        if (position != ++i)
        {
            continue;
        }
        
        yield return newValue;
    }
}

private static IReadOnlyList<int> ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',').Select(int.Parse).ToArray();
    
private class Program
{
    private readonly int[] code;
    private int ins;

    private Program(int[] code)
    {
        this.code = code;
        this.ins = 0;
        this.IsHalted = false;
    }
    
    public bool IsHalted { get; private set; }
    
    public static Program Create(IReadOnlyList<int> code) => new Program(code.ToArray());
    
    public IReadOnlyList<int> Run(IEnumerable<int> userInput)
    {
        if (this.IsHalted)
        {
            throw new InvalidOperationException("Can't run: the program has halted before.");
        }
    
        var inputQueue = new Queue<int>(userInput);
        var outputStream = new List<int>();
        
        while (true)
        {
            Command cmd = Command.Read(this.code, ins);
            int continueFrom = this.ins + cmd.Args.Count + 1;
            
            switch (cmd.OpCode)
            {
                case OpCode.Add:
                    this.code[cmd.Args[2]] = cmd.GetArgVal(this.code, 0) + cmd.GetArgVal(this.code, 1);
                    break;
                    
                case OpCode.Multiply:
                    this.code[cmd.Args[2]] = cmd.GetArgVal(this.code, 0) * cmd.GetArgVal(this.code, 1);
                    break;
                    
                case OpCode.Input:
                    if (inputQueue.Count is 0)
                    {
                        return outputStream;
                    }
                
                    this.code[cmd.Args[0]] = inputQueue.Dequeue();
                    break;
                    
                case OpCode.Output:
                    outputStream.Add(this.code[cmd.Args[0]]);
                    break;
                    
                case OpCode.JumpIfTrue:
                    if (cmd.GetArgVal(this.code, 0) > 0)
                    {
                        continueFrom = cmd.GetArgVal(this.code, 1);
                    }
                
                    break;
                    
                case OpCode.JumpIfFalse:
                    if (cmd.GetArgVal(this.code, 0) is 0)
                    {
                        continueFrom = cmd.GetArgVal(this.code, 1);
                    }
                
                    break;
                    
                case OpCode.LessThan:
                    this.code[cmd.Args[2]] = (cmd.GetArgVal(this.code, 0) < cmd.GetArgVal(this.code, 1) ? 1 : 0);
                    break;
                    
                case OpCode.Equals:
                    this.code[cmd.Args[2]] = (cmd.GetArgVal(this.code, 0) == cmd.GetArgVal(this.code, 1) ? 1 : 0);
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
    
private record struct Command(OpCode OpCode, IReadOnlyList<int> Args, IReadOnlyList<int> ArgModes)
{
    public static Command Read (ReadOnlySpan<int> program, int i)
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
    
    public int GetArgVal(IReadOnlyList<int> program, int index)
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
