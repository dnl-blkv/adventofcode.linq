<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int[] input = ParseInput(GetInput());
    Solve(input).Dump();
}

private const int NounPosition = 1;
private const int MinNoun = 0;
private const int MaxNoun = 99;
private const int Noun1 = 12;

private const int VerbPosition = 2;
private const int MinVerb = 0;
private const int MaxVerb = 99;
private const int Verb1 = 2;

private const int CodeAdd = 1;
private const int CodeMultiply = 2;
private const int CodeHalt = 99;
private const int ParamCount = 3;

private const int DesiredOuput2 = 1969_07_20;
private const int NounMultiplier2 = 100;

private static (long Result1, long Result2) Solve(int[] input)
{
    long result1 = -1;
    long result2 = -1;

    for (int noun = MinNoun; noun <= MaxNoun; noun++)
    {
        for (int verb = MinVerb; verb <= MaxVerb; verb++)
        {
            long output = RunIntcodeProgram(input, noun: noun, verb: verb);
            
            if (noun is Noun1 && verb is Verb1)
            {
                result1 = output;
                continue;
            }
            
            if (output is DesiredOuput2)
            {
                result2 = NounMultiplier2 * noun + verb;
                continue;
            }
        
            if (result1 < 0 || result2 < 0)
            {
                continue;
            }
            
            break;
        }
    }
    
    return (Result1: result1, Result2: result2);
}

private static long RunIntcodeProgram(int[] input, int noun, int verb)
{   
    Span<int> program = input.ToArray().AsSpan<int>();
    program[NounPosition] = noun;
    program[VerbPosition] = verb;
    int i = 0;
    
    while (true)
    {
        Span<int> args = ParseCommand(program, i, out int opCode);
        
        switch (opCode)
        {
            case CodeAdd:
                program[args[2]] = program[args[0]] + program[args[1]];
                break;
                
            case CodeMultiply:
                program[args[2]] = program[args[0]] * program[args[1]];
                break;
                
            case CodeHalt:
                return program[0];
                
            default:
                throw new InvalidOperationException($"Unexpected opCode: '{opCode}'");
        }
        
        i += args.Length + 1;
    }
    
    static Span<int> ParseCommand(Span<int> program, int i, out int opCode)
    {
        int start = i + 1;
        opCode = program[i];
        
        return program[start..(start + ParamCount)];
    }
}

private static int[] ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',').Select(int.Parse).ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
