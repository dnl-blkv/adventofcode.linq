<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Action<Memory>[] input = ParseInput(GetInput());
	Solve(input).Dump();
}

private const int MemSize = 16;
private const int Iterations2 = 1_000_000_000;

private static (string Result1, string Result2) Solve(Action<Memory>[] input)
{
    int k = 0;
    var mem = new Memory();
    string initialState = mem.ToString();
    string currentState = initialState;
    string? result1 = null;
    var visited = new HashSet<(string State, int ActionIndex)>();
    
    do
    {
        int actionIndex = k % input.Length;
        input[actionIndex](mem);
        currentState = mem.ToString();
        k++;
        
        if (result1 is null && k == input.Length)
        {
            result1 = currentState;
        }
        
        if (!visited.Add((State: currentState, ActionIndex: actionIndex)))
        {
            break;
        }
    } while (currentState != initialState);
    
    for (int i = 0; i < Iterations2 % k; i++)
    {
        input[i % input.Length](mem);
    }
    
    return (Result1: result1!, Result2: mem.ToString());
}

private static Action<Memory>[] ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',')
        .Select(ins =>
        {
            char prefix = ins[0];
            string[] args = ins[1..].Split('/');
        
            switch (prefix)
            {
                case 's':
                    int l = int.Parse(args[0]);
                    return (Action<Memory>)(mem => mem.Spin(l));
                    
                case 'x':
                    int[] poss = args.Select(int.Parse).ToArray();
                    return mem => mem.Exchange(poss[0], poss[1]);
                    
                case 'p':
                    char[] chars = args.Select(s => s[0]).ToArray();
                    return mem => mem.Partner(chars[0], chars[1]);
                    
                default:
                    throw new InvalidOperationException($"Unknown instruction prefix: {prefix}");
            }
        })
        .ToArray();

private class Memory
{
    private readonly Dictionary<char, int> charMap;
    private char[] mem;
    
    public Memory()
    {
        this.mem = Enumerable.Range(0, MemSize).Select(i => (char)('a' + i)).ToArray();
        this.charMap = [];
        this.RefreshCharMap();
    }
    
    public void Spin(int l)
    {
        ReadOnlySpan<char> memSpan = this.mem.AsSpan();
        this.mem = string.Concat(memSpan[^l..], memSpan[..^l]).ToArray();
        this.RefreshCharMap();
    }
    
    public void Exchange(int posA, int posB)
    {
        (char charA, char charB) = (this.mem[posA], this.mem[posB]);
        Swap(posA, charA, posB, charB);
    }
    
    public void Partner(char charA, char charB)
    {
        (int posA, int posB) = (this.charMap[charA], this.charMap[charB]);
        Swap(posA, charA, posB, charB);
    }
    
    public override string ToString() => string.Join(string.Empty, this.mem);
    
    private void RefreshCharMap()
    {
        foreach ((char c, int i) in this.mem.Select((c, i) => (c, i)))
        {
            this.charMap[c] = i;
        }
    }
    
    private void Swap(int posA, char charA, int posB, char charB)
    {
        (this.mem[posA], this.mem[posB]) = (this.mem[posB], this.mem[posA]);
        (this.charMap[charA], this.charMap[charB]) = (this.charMap[charB], this.charMap[charA]);
    }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}