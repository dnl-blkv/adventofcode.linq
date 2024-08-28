<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (IReadOnlyList<Sample> samples, IReadOnlyList<int[]> program) = ParseInput(GetInput());
	Solve(samples, program).Dump();
}

private delegate void Operation(int[] reg, int a, int b, int c);

private static readonly IReadOnlyList<Operation> Operations =
[
    (reg, a, b, c) => reg[c] = reg[a] + reg[b],
    (reg, a, b, c) => reg[c] = reg[a] + b,
    (reg, a, b, c) => reg[c] = reg[a] * reg[b],
    (reg, a, b, c) => reg[c] = reg[a] * b,
    (reg, a, b, c) => reg[c] = reg[a] & reg[b],
    (reg, a, b, c) => reg[c] = reg[a] & b,
    (reg, a, b, c) => reg[c] = reg[a] | reg[b],
    (reg, a, b, c) => reg[c] = reg[a] | b,
    (reg, a, b, c) => reg[c] = reg[a],
    (reg, a, b, c) => reg[c] = a,
    (reg, a, b, c) => reg[c] = (a > reg[b] ? 1 : 0),
    (reg, a, b, c) => reg[c] = (reg[a] > b ? 1 : 0),
    (reg, a, b, c) => reg[c] = (reg[a] > reg[b] ? 1 : 0),
    (reg, a, b, c) => reg[c] = (a == reg[b] ? 1 : 0),
    (reg, a, b, c) => reg[c] = (reg[a] == b ? 1 : 0),
    (reg, a, b, c) => reg[c] = (reg[a] == reg[b] ? 1 : 0)
];

private static (long Result1, long Result2) Solve(IReadOnlyList<Sample> samples, IReadOnlyList<int[]> program)
{
    Dictionary<int, HashSet<int>> filter =
        Enumerable.Range(0, Operations.Count)
            .ToDictionary(
                i => i,
                _ => Enumerable.Range(0, Operations.Count).ToHashSet());
                
    int result1 = 0;
    
    foreach (Sample sample in samples)
    {
        int candidateOpCodesCount = 0;
        filter[sample.Op].IntersectWith(
            Operations
                .Select((op, i) => (op, i))
                .Where(t => sample.Validate(t.op))
                .Select(t => 
                {
                    candidateOpCodesCount++;
                    return t.i;
                }));
        result1 += (candidateOpCodesCount >= 3 ? 1 : 0);
    }
    
    int[] opMap = Enumerable.Repeat(-1, Operations.Count).ToArray();
    
    while (filter.Count > 0)
    {
        (int sampleOp, HashSet<int> targetOps) = filter.Where(kv => kv.Value.Count == 1).First();
        int targetOp = targetOps.Single();
        opMap[sampleOp] = targetOp;
        filter.Remove(sampleOp);
        
        foreach ((int _, HashSet<int> opsToClean) in filter)
        {
            opsToClean.Remove(targetOp);
        }
    }
    
    int[] reg = [0, 0, 0, 0];
    
    foreach (int[] args in program)
    {
        Operations[opMap[args[0]]](reg, args[1], args[2], args[3]);
    }
    
    return (Result1: result1, Result2: reg[0]);
}

private static (IReadOnlyList<Sample> Samples, IReadOnlyList<int[]> Program) ParseInput(IEnumerable<string> input)
{
    List<string> testCaseLines = [];
    List<Sample> samples = [];
    List<int[]> program = [];

    foreach (string line in input)
    {
        if (line.Length is 0)
        {
            if (testCaseLines.Count > 0)
            {
                samples.Add(Sample.Parse(testCaseLines));
                testCaseLines = [];
            }
        
            continue;
        }
        
        if (char.IsLetter(line[0]) || testCaseLines.Count > 0)
        {
            testCaseLines.Add(line);
            continue;
        }
        
        program.Add(ParseProgramLine(line));
    }

    return (samples, program);
}
        
private static int[] ParseProgramLine(string line) =>
    ToTntArray(line.Split(' '));
        
private static int[] ToTntArray(IEnumerable<string> intStrings) =>
    intStrings.Select(int.Parse).ToArray();

private record struct Sample
{
    private readonly IReadOnlyList<int> before;
    private readonly IReadOnlyList<int> args;
    private readonly IReadOnlyList<int> after;

    public Sample(IReadOnlyList<int> before, IReadOnlyList<int> args, IReadOnlyList<int> after)
    {
        this.before = before;
        this.args = args;
        this.after = after;
    }

    public int Op => this.args[0];
    
    private int A => this.args[1];
    
    private int B => this.args[2];
    
    private int C => this.args[3];

    public static Sample Parse(IReadOnlyList<string> testCaseLines)
    {
        return new Sample(
            before: ParseBeforeAfter(testCaseLines[0]),
            args: ParseProgramLine(testCaseLines[1]),
            after: ParseBeforeAfter(testCaseLines[2]));
            
        static int[] ParseBeforeAfter(string line) =>
            ToTntArray(line[9..^1].Split(", "));
    }
    
    public bool Validate(Operation operation)
    {
        int[] reg = this.before.ToArray();
        operation(reg, this.A, this.B, this.C);
        
        return RegEq(reg, this.after);
    }
    
    private bool RegEq(IReadOnlyList<int> a, IReadOnlyList<int> b) =>
        a.Count is not 4 || a.Count != b.Count
            ? throw new ArgumentException(
                $"Expected both counts to be 4, but got a.Count = {a.Count}, b.Count = {b.Count}.")
            : a.Zip(b).All(t => t.First == t.Second);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}