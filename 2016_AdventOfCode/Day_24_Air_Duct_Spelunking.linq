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

private static readonly IReadOnlyList<Point> Neighbors =
[
    new Point(I:  0, J:  1),
    new Point(I:  1, J:  0),
    new Point(I:  0, J: -1),
    new Point(I: -1, J:  0)
];

private const int NoneMask = (int)0b00000001;
private const int AllMask = (int)0b11111111;

private static (long Result1, long Result2) Solve(string[] input)
{
    (Point start, IReadOnlyDictionary<Point, int> toVisit) = GetNumPoints(input);
    (Point Pos, int Mask) startState = (Pos: start, Mask: NoneMask);
    HashSet<(Point Pos, int Mask)> currentStates = [startState];
    var distances = new Dictionary<(Point Pos, int Mask), int>();
    int steps = 1;
    long? result1 = null;
    
    while (currentStates.Count > 0)
    {
         HashSet<(Point Pos, int Mask)> nextStates = [];
        
        foreach ((Point currentPos, int currentMask) in currentStates)
        {
            foreach (Point nextPos in Neighbors.Select(n => currentPos + n))
            {
                if (input[nextPos.I][nextPos.J] is '#')
                {
                    continue;
                }
                
                int nextMask =
                    toVisit.TryGetValue(nextPos, out int nextSubMask)
                        ? (currentMask | (1 << nextSubMask))
                        : currentMask;
                        
                if (nextMask is AllMask)
                {
                    result1 ??= steps;
                    
                    if (nextPos == start)
                    {
                        return (Result1: result1!.Value, Result2: steps);
                    }
                }
                        
                (Point Pos, int Mask) candidateKey = (nextPos, nextMask);
                
                if (distances.TryGetValue(candidateKey, out int oldSteps) && steps >= oldSteps)
                {
                    continue;
                }
                
                distances[candidateKey] = steps;
                nextStates.Add((nextPos, nextMask));
            }
        }
        
        currentStates = nextStates;
        steps++;
    }
    
    return (Result1: -1, Result2: -1);
}

private static (Point Start, IReadOnlyDictionary<Point, int> ToVisit) GetNumPoints(string[] input)
{
    Point? start = null;
    var toVisit = new Dictionary<Point, int>();

    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[i].Length; j++)
        {
            if (!char.IsDigit(input[i][j]))
            {   
                continue;
            }
            
            int currentPointIndex = input[i][j] - '0';
            var currentPoint = new Point(I: i, J: j);
            
            if (currentPointIndex == 0)
            {
                start = currentPoint;
                continue;
            }
            
            toVisit.Add(currentPoint, currentPointIndex);
        }
    }
    
    return (Start: start!, ToVisit: toVisit);
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private record class Point(int I, int J)
{
    public static Point operator +(Point a, Point b) =>
        new Point(I: a.I + b.I, J: a.J + b.J);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}