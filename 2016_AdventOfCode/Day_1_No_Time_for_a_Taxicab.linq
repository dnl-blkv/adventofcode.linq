<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (char T, int C)[] input = ParseInput(GetInput());
	Solve(input).Dump();
}

private static readonly IReadOnlyDictionary<(char D, char T), char> TurnTable =
    new Dictionary<(char D, char T), char>
    {
        [(D: 'U', T: 'L')] = 'L',
        [(D: 'U', T: 'R')] = 'R',
        [(D: 'R', T: 'L')] = 'U',
        [(D: 'R', T: 'R')] = 'D',
        [(D: 'D', T: 'L')] = 'R',
        [(D: 'D', T: 'R')] = 'L',
        [(D: 'L', T: 'L')] = 'D',
        [(D: 'L', T: 'R')] = 'U'
    };
    
private static readonly IReadOnlyDictionary<char, (int Di, int Dj)> MoveTable =
    new Dictionary<char, (int Di, int Dj)>
    {
        ['U'] = (Di: -1, Dj:  0),
        ['R'] = (Di:  0, Dj:  1),
        ['D'] = (Di:  1, Dj:  0),
        ['L'] = (Di:  0, Dj: -1),
    };

private static (long Result1, long Result2) Solve((char T, int C)[] input)
{
    char d = 'U';
    var i = 0;
    var j = 0;
    HashSet<(int I, int J)> visited = [(i, j)];
    int result2 = -1;
    
    foreach ((char t, int c) in input)
    {
        d = TurnTable[(d, t)];
        (int dI, int dJ) = MoveTable[d];
        
        for (int k = 0; k < c; k++)
        {
            i += dI;
            j += dJ;
        
            if (result2 >= 0 || visited.Add((i, j)))
            {
                continue;
            }
            
            result2 = Dist();
        }
    }
    
    return (Result1: Dist(), Result2: result2);
    
    int Dist() => Math.Abs(i) + Math.Abs(j);
}

private static (char D, int C)[] ParseInput(IEnumerable<string> input) =>
    input.First().Split(", ").Select(s => (D: s[0], C: int.Parse(s[1..]))).ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}