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

private const int MinDoorCount2 = 1_000;

private static (long Result1, long Result2) Solve(string input)
{
    IReadOnlyDictionary<Point, HashSet<Point>> map = BuildMap(input);
    Point startState = Point.Zero;
    HashSet<Point> currentStates = [startState];
    HashSet<Point> visited = [startState];
    int result1 = 0;
    int result2 = 0;
    
    while (true)
    {
        currentStates = currentStates.SelectMany(p => map[p]).Where(visited.Add).ToHashSet();
        
        if (currentStates.Count > 0)
        {
            result1++;
            result2 += (result1 >= MinDoorCount2 ? currentStates.Count : 0);
            continue;
        }
        
        return (Result1: result1, Result2: result2);
    }
}

private static IReadOnlyDictionary<Point, HashSet<Point>> BuildMap(string input)
{
    Point startPoint = Point.Zero;
    NfaState startNfaState = NfaState.BuildNfa(input);
    IReadOnlyList<(Point Point, NfaState NfaState)> currentStates = [(Point: startPoint, NfaState: startNfaState)];
    HashSet<NfaState> visited = [startNfaState];
    Dictionary<Point, HashSet<Point>> map = [];
    
    while (currentStates.Count > 0)
    {
        List<(Point Point, NfaState NfaState)> nextStates = [];
        
        foreach ((Point currentPoint, NfaState currentNfaState) in currentStates)
        {
            foreach (NfaState nextNfaState in currentNfaState.Next!.Where(visited.Add))
            {
                Point nextPoint = currentPoint + nextNfaState.Delta;
                
                if (nextPoint != currentPoint)
                {
                    AddDoor(currentPoint, nextPoint);
                }
                
                if (nextNfaState.IsFinal)
                {
                    continue;
                }
                
                nextStates.Add((Point: nextPoint, NfaState: nextNfaState));
            }
        }
    
        currentStates = nextStates;
    }
    
    return map;
    
    void AddDoor(Point from, Point to)
    {
        AddEdge(from, to);
        AddEdge(to, from);
    }
    
    void AddEdge(Point from, Point to)
    {
        if (!map.ContainsKey(from))
        {
            map[from] = [];
        }
        
        map[from].Add(to);
    }
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private class NfaState
{
    private static readonly IReadOnlyDictionary<char, Point> Deltas =
        new Dictionary<char, Point>
        {
            ['^'] = Point.Zero,
            ['E'] = new Point(X:  1, Y:  0),
            ['S'] = new Point(X:  0, Y:  1),
            ['W'] = new Point(X: -1, Y:  0),
            ['N'] = new Point(X:  0, Y: -1),
            ['$'] = Point.Zero
        };

    public Point Delta { get; init; } = Point.Zero;
    
    public IReadOnlyList<NfaState>? Next { get; set; }
    
    public bool IsFinal => Next is null;
    
    public static NfaState BuildNfa(string input)
    {
        SortedDictionary<int, int> parentheses = FindParenthesesPairs(input);
        
        return BuildNfa(input, startAt: 0, endBefore: input.Length).Start;
        
        (NfaState Start, NfaState End) BuildNfa(string input, int startAt, int endBefore)
        {
            IReadOnlyList<(int StartAt, int EndBefore)> branches =
                FindBranches(input, startAt: startAt, endBefore: endBefore).ToArray();
            
            if (branches.Count > 1)
            {
                var endNfaState = new NfaState();
                var newBranchingNfaState = new NfaState
                {
                    Next = branches
                        .Select(t =>
                        { 
                            (NfaState branchingStart, NfaState branchingEnd) =
                                BuildNfa(input, startAt: t.StartAt, endBefore: t.EndBefore);
                            branchingEnd.Next = [endNfaState];
                            
                            return branchingStart;
                        })
                        .ToArray()
                };
            
                return (Start: newBranchingNfaState, End: endNfaState);
            }
            
            var firstNfaState = new NfaState();
            NfaState currentNfaState = firstNfaState;
            
            for (int i = startAt; i < endBefore; i++)
            {
                int nextParenthesisIndex = input.IndexOf('(', startIndex: i, count: endBefore - i);
                
                if (nextParenthesisIndex < 0)
                {
                    currentNfaState = currentNfaState.AppendValueChain(value: input[i..endBefore]);
                    break;
                }
                
                if (nextParenthesisIndex > i)
                {
                    currentNfaState = currentNfaState.AppendValueChain(value: input[i..nextParenthesisIndex]);
                    i = nextParenthesisIndex - 1;
                    continue;
                }
                
                int closingParenthesisIndex = parentheses[i];
                (NfaState newSubFsmStart, NfaState newSubFsmEnd) =
                    BuildNfa(input, startAt: i + 1, endBefore: closingParenthesisIndex);
                currentNfaState.Next = [newSubFsmStart];
                currentNfaState = newSubFsmEnd;
                i = closingParenthesisIndex;
            }
        
            return (Start: firstNfaState, End: currentNfaState);
        }
        
        IEnumerable<(int StartAt, int EndBefore)> FindBranches(string input, int startAt, int endBefore)
        {
            int branchStart = startAt;
        
            for (int i = startAt; i < endBefore; i++)
            {
                if (parentheses.ContainsKey(i))
                {
                    i = parentheses[i];
                    continue;
                }
                
                if (input[i] is not '|')
                {
                    continue;
                }
                
                yield return (StartAt: branchStart, EndBefore: i);
                branchStart = i + 1;
            }
            
           yield return (StartAt: branchStart, EndBefore: endBefore);
        }
    }
    
    public NfaState AppendValueChain(string value)
    {
        NfaState currentNfaState = this;
        
        foreach (char c in value)
        {
            var newNfaState = new NfaState
            {
                Delta = Deltas[c]
            };
            currentNfaState.Next = [newNfaState];
            currentNfaState = newNfaState;
        }
        
        return currentNfaState;
    }
    
    private static SortedDictionary<int, int> FindParenthesesPairs(string input)
    {
        SortedDictionary<int, int> parentheses = [];
        Stack<int> openParentheses = [];
        
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            
            switch (c)
            {
                case '(':
                    openParentheses.Push(i);
                    break;
                    
                case ')':
                    parentheses[openParentheses.Pop()] = i;
                    break;
                    
                default:
                    break;
            }
        }
        
        return parentheses;
    }
}

private record struct Point(int X, int Y)
{
    public static readonly Point Zero = new Point(X: 0, Y: 0);

    public static Point operator +(Point a, Point b) =>
        new Point(X: a.X + b.X, Y: a.Y + b.Y);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}