<Query Kind="Program">
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<Instruction> input = ParseInput(GetInput());
	Solve(input).Dump();
}

private static (int Result1, int Result2) Solve(IReadOnlyList<Instruction> input)
{
    int result1 = CompileProgram(input)["a"]();
    
    List<Instruction> input2 = input.Where(instruction => instruction.Name != "b").ToList();
    input2.Add(new Instruction(name: "b", type: OperationTypes.ASSIGN, left: null, right: new Operand(result1)));

    return (Result1: CompileProgram(input)["a"](), Result2: CompileProgram(input2)["a"]());
}

private static IReadOnlyDictionary<string, Func<int>> CompileProgram(IEnumerable<Instruction> input)
{
    var program = new Dictionary<string, Func<int>>();
    
    foreach (Instruction instruction in input)
    {
        program[instruction.Name] = CompileInstruction(instruction);
    }
    
    return program;
    
    Func<int> CompileInstruction(Instruction instruction)
    {
        int? cachedResult = null;
        Func<int> rawFunc = instruction.Type switch
        {
            OperationTypes.ASSIGN => () => Deref(instruction.Right!),
            OperationTypes.NOT    => () => (~Deref(instruction.Right!)),
            OperationTypes.AND    => () => (Deref(instruction.Left!) & Deref(instruction.Right!)),
            OperationTypes.OR     => () => (Deref(instruction.Left!) | Deref(instruction.Right!)),
            OperationTypes.LSHIFT => () => (Deref(instruction.Left!) << Deref(instruction.Right!)),
            OperationTypes.RSHIFT => () => (Deref(instruction.Left!) >> Deref(instruction.Right!)),
            _ => throw new Exception("Should never happen!")
        };
        
        return () => (cachedResult ?? (cachedResult = (int)ushort.MaxValue & rawFunc())).Value;
    }
    
    int Deref(Operand operand) => operand.Value ?? program[operand.Name!]();
}
        
private static IReadOnlyList<Instruction> ParseInput(IEnumerable<string> input) =>
    input.Select(Instruction.Parse).ToArray();

private class Instruction
{
    public Instruction(string name, string type, Operand? left = null, Operand? right = null)
    {
        this.Name = name;
        this.Type = type;
        this.Left = left;
        this.Right = right;
    }

    public string Name { get; }
    
    public string Type { get; }
    
    public Operand? Left { get; }
    
    public Operand? Right { get; }
    
    public static Instruction Parse(string instructionString)
    {
        string[] instructionParts = instructionString.Split(' ');
        
        return instructionParts.Length switch
        {
            3 => new Instruction(
                name: instructionParts[2],
                type: OperationTypes.ASSIGN,
                left: null,
                right: Operand.Parse(instructionParts[0])),
            4 => new Instruction(
                name: instructionParts[3],
                type: OperationTypes.NOT,
                left: null,
                right: Operand.Parse(instructionParts[1])),
            5 => new Instruction(
                name: instructionParts[4],
                type: instructionParts[1],
                left: Operand.Parse(instructionParts[0]),
                right: Operand.Parse(instructionParts[2])),
            _ => throw new Exception("Should never happen!")
        };
    }
}

private class Operand
{
    public Operand(string name) => this.Name = name;
    
    public Operand(int value) => this.Value = value;
    
    public string? Name { get; }
    
    public int? Value { get; }
    
    public static Operand Parse(string operandString) =>
        int.TryParse(operandString, out int value)
            ? new Operand(value)
            : new Operand(operandString);
}

private static class OperationTypes
{
    public const string ASSIGN = nameof(ASSIGN);
    public const string NOT = nameof(NOT);
    public const string AND = nameof(AND);
    public const string OR = nameof(OR);
    public const string LSHIFT = nameof(LSHIFT);
    public const string RSHIFT = nameof(RSHIFT);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}