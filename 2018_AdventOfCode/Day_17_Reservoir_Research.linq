<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<Point> input = ParseInput(GetInput());
    Solve(input).Dump();
}

private static readonly Point Source = new Point(Y: 0, X: 500);

private static readonly (Point Right, Point Down, Point Left, Point Up) Delta =
(
    Right: new Point(Y:  0, X:  1),
    Down:  new Point(Y:  1, X:  0),
    Left:  new Point(Y:  0, X: -1),
    Up:    new Point(Y: -1, X:  0)
);

private static (long Result1, long Result2) Solve(IReadOnlyList<Point> input)
{
    int maxY = input.Max(p => p.Y);
    HashSet<Point> taken = input.ToHashSet();
    HashSet<Point> visited = [];
    HashSet<Point> atRest = [];
    HashSet<Point> currentStates = [Source];
    
    while (currentStates.Count > 0)
    {
        HashSet<Point> nextStates = [];
    
        foreach (Point source in currentStates)
        {
            if (TryFlowDown(source, out Point currentPosition))
            {
                continue;
            }
            
            for (;; currentPosition += Delta.Up)
            {
                bool flowsLeft = TryFlowSideways(currentPosition, stepDelta: Delta.Left, out Point leftBoundary);
                bool flowsRight = TryFlowSideways(currentPosition, stepDelta: Delta.Right, out Point rightBoundary);
                
                if (!flowsLeft && !flowsRight)
                {
                    for (int x = leftBoundary.X; x <= rightBoundary.X; x++)
                    {
                        var p = new Point(Y: currentPosition.Y, X: x);
                        taken.Add(p);
                        atRest.Add(p);
                    }
                    
                    continue;
                }
                
                if (flowsLeft)
                {
                    nextStates.Add(leftBoundary);
                }
                
                if (flowsRight)
                {
                    nextStates.Add(rightBoundary);
                }
                
                break;
            }
        }
        
        currentStates = nextStates;
    }
    
    int minY = input.Min(p => p.Y);
    
    return (Result1: visited.Count(p => p.Y >= minY), Result2: atRest.Count);
    
    bool TryFlowDown(Point startFrom, out Point nextPosition)
    {
        nextPosition = startFrom;
    
        while (!taken.Contains(nextPosition + Delta.Down))
        {
            nextPosition += Delta.Down;
            
            if (nextPosition.Y > maxY)
            {
                return true;
            }
            
            visited.Add(nextPosition);
        }
        
        return false;
    }
    
    bool TryFlowSideways(Point startFrom, Point stepDelta, out Point nextState)
    {
        visited.Add(startFrom);
        nextState = startFrom;
        
        while (!taken.Contains(nextState + stepDelta))
        {
            visited.Add(nextState += stepDelta);
        
            if (taken.Contains(nextState + Delta.Down))
            {
                continue;
            }
            
            return true;
        }
        
        return false;
    }
}

private static void PrintField(IReadOnlyList<Point> input, HashSet<Point> visited)
{
    var resultBuilder = new StringBuilder();

    int minY = input.Concat(visited).Append(Source).Min(p => p.Y);
    int maxY = input.Concat(visited).Append(Source).Max(p => p.Y);
    int ySize = maxY - minY + 1;
    
    int minX = input.Concat(visited).Append(Source).Min(p => p.X);
    int maxX = input.Concat(visited).Append(Source).Max(p => p.X);
    int xSize = maxX - minX + 1;
    
    char[][] field = Enumerable.Range(0, ySize).Select(_ => Enumerable.Repeat('.', xSize).ToArray()).ToArray();
    
    foreach (Point p in input)
    {
        field[p.Y - minY][p.X - minX] = '#';
    }
    
    foreach (Point p in visited)
    {
        field[p.Y - minY][p.X - minX] = '/';
    }
    
    field[Source.Y - minY][Source.X - minX] = '+';
    
    foreach (char[] line in field)
    {
        resultBuilder.AppendLine(string.Join(string.Empty, line));
    }
    
    resultBuilder.ToString().Dump();
}

private static IReadOnlyList<Point> ParseInput(IEnumerable<string> input)
{
    List<Point> clay = [];

    foreach (string line in input)
    {
        IReadOnlyDictionary<char, (int min, int max)> lineParts =
            line.Split(", ").ToDictionary(p => p[0], p => ParseRange(p[2..]));
        (int minY, int maxY) = lineParts['y'];
        (int minX, int maxX) = lineParts['x'];
        
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                clay.Add(new Point(Y: y, X: x));
            }
        }
    }
    
    return clay;
    
    static (int min, int max) ParseRange(string rangeString)
    {
        int[] rangeSides = rangeString.Split("..").Select(int.Parse).ToArray();
        
        return (min: rangeSides[0], max: rangeSides[^1]);
    }
}

private record struct Point(int Y, int X)
{
    public static Point operator +(Point a, Point b) =>
        new Point(Y: a.Y + b.Y, X: a.X + b.X);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
