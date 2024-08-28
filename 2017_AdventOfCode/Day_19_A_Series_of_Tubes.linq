<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
    Solve(input).Dump();
}

private const char Turn = '+';
private const char Vert = '|';
private const char Hori = '-';
private const char Empt = ' ';

private static readonly IReadOnlyList<Point> Directions =
[
    new Point(I:  0, J:  1),
    new Point(I:  1, J:  0),
    new Point(I:  0, J: -1),
    new Point(I: -1, J:  0)
];
private static readonly int DirectionsPerDimension = Directions.Count / 2;

private static (string Result1, int Result2) Solve(string[] input)
{
    var currentPos = new Point(I: 0, J: input[0].IndexOf(Vert));
    int dir = 1;
    var pathBuilder = new StringBuilder();
    int dist = 0;
    
    while (input[currentPos.I][currentPos.J] != Empt)
    {
        char currentChar = input[currentPos.I][currentPos.J];
        
        if (currentChar == Turn)
        {
            dir = Enumerable.Range(0, DirectionsPerDimension)
                .Select(i => 2 * i + 1 - (dir % 2))
                .Where(d =>
                {
                    Point neighborPos = currentPos + Directions[d];
                    
                    if (neighborPos.I < 0 || neighborPos.I >= input.Length
                        || neighborPos.J < 0 || neighborPos.J >= input[neighborPos.I].Length)
                    {
                        return false;
                    }
                    
                    char neighborChar = input[neighborPos.I][neighborPos.J];
                    
                    return neighborChar != Empt && neighborChar != (d % 2 == 0 ? Vert : Hori);
                })
                .Single();
        }
        else if (char.IsLetter(currentChar))
        {
            pathBuilder.Append(currentChar);
        }
     
        currentPos += Directions[dir];
        dist++;
    }
    
    return (Result1: pathBuilder.ToString(), Result2: dist); 
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();
    
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
