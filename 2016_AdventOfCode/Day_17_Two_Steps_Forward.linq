<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
	Solve(input).Dump();
}

private static (string Result1, long Result2) Solve(string input)
{
    var initialState = new State(Pos: new Point(I: 0, J: 0), Path: string.Empty);
    var finalPos = new Point(I: 3, J: 3);
    HashSet<State> currentStates = [initialState];
    using MD5 md5 = MD5.Create();
    int l = 0;
    string? result1 = null;
    int result2 = 0;
    
    while (currentStates.Count > 0)
    {
        State? finalState = null;
        currentStates =
            currentStates
                .SelectMany(s => s.GetNextStates(md5, input))
                .Where(s =>
                {
                    if (s.Pos != finalPos)
                    {
                        return true;
                    }
                    
                    finalState ??= s;
                    return false;
                })
                .ToHashSet();
        l++;
        
        if (finalState is null)
        {
            continue;
        }
        
        result1 ??= finalState.Value.Path;
        result2 = l;
    }
    
    return (result1!, result2);
}

private static string CreateHash(MD5 md5, string input)
{
    byte[] inputBytes = Encoding.UTF8.GetBytes(input);
    byte[] hashBytes = md5.ComputeHash(inputBytes);
    
    return Convert.ToHexString(hashBytes).ToLower();
}

private static string ParseInput(IEnumerable<string> input) => input.Single();
    
private record struct State(Point Pos, string Path)
{
    public IEnumerable<State> GetNextStates(MD5 md5, string input)
    {
        string hash = CreateHash(md5, $"{input}{this.Path}");
        
        if (IsOpen(hash[0]) && this.Pos.I > 0)
        {
            yield return new State(Pos: this.Pos + new Point(I: -1, J: 0), Path: $"{this.Path}U");
        }
        
        if (IsOpen(hash[1]) && this.Pos.I < 3)
        {
            yield return new State(Pos: this.Pos + new Point(I: 1, J: 0), Path: $"{this.Path}D");
        }
        
        if (IsOpen(hash[2]) && this.Pos.J > 0)
        {
            yield return new State(Pos: this.Pos + new Point(I: 0, J: -1), Path: $"{this.Path}L");
        }
        
        if (IsOpen(hash[3]) && this.Pos.J < 3)
        {
            yield return new State(Pos: this.Pos + new Point(I: 0, J: 1), Path: $"{this.Path}R");
        }
        
        static bool IsOpen(char c) =>
            c is >= 'b' and <= 'f';
    }
}
    
private record struct Point(int I, int J)
{
    public static Point operator +(Point a, Point b) =>
        new Point(I: a.I + b.I, J: a.J + b.J);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}