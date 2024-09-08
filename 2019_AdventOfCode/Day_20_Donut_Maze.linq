<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (
        State initialState,
        State finalState,
        IReadOnlyDictionary<Point, HashSet<StateTransition>> transitions) = ParseInput(GetInput());
    
    Solve1(initialState: initialState, finalState: finalState, transitions: transitions).Dump();
    Solve2(initialState: initialState, finalState: finalState, transitions: transitions).Dump();
}

private const char CellPassage = '.';

private const int FieldMargin = 2;

private const string StartName = "AA";
private const string EndName = "ZZ";

private static readonly IReadOnlyList<Point> Directions =
[
    new Point(Y:  0, X:  1),
    new Point(Y:  1, X:  0),
    new Point(Y:  0, X: -1),
    new Point(Y: -1, X:  0)
];

private static long Solve1(
    State initialState,
    State finalState,
    IReadOnlyDictionary<Point, HashSet<StateTransition>> transitions) =>
    Solve(initialState: initialState, finalState: finalState, transitions: transitions, levelDeltaMultiplier: 0);

private static long Solve2(
    State initialState,
    State finalState,
    IReadOnlyDictionary<Point, HashSet<StateTransition>> transitions) =>
    Solve(initialState: initialState, finalState: finalState, transitions: transitions, levelDeltaMultiplier: 1);

private static long Solve(
    State initialState,
    State finalState,
    IReadOnlyDictionary<Point, HashSet<StateTransition>> transitions,
    int levelDeltaMultiplier)
{   
    HashSet<State> currentStates = [initialState];
    HashSet<State> visited = [initialState];
    int dist = 0;
    
    do
    {
        currentStates =
            currentStates
                .SelectMany(s =>
                    transitions[s.Point].Select(t =>
                        new State(Point: t.NextPoint, Level: s.Level + t.LevelDelta * levelDeltaMultiplier)))
                .Where(s => s.Level >= 0 && visited.Add(s))
                .ToHashSet();
        dist++;
    }
    while (!currentStates.Contains(finalState));
    
    return dist;
}

private static (
    State InitialState,
    State FinalState,
    IReadOnlyDictionary<Point, HashSet<StateTransition>> Transitions) ParseInput(IEnumerable<string> input)
{
    IReadOnlyList<string> field = input.ToList();
    int fieldHeight = field.Count;
    int fieldWidth = field[0].Length;
    Dictionary<Point, HashSet<StateTransition>> transitions = [];
    Dictionary<string, HashSet<Point>> namedPoints = [];

    foreach ((Point currentPoint, char currentChar) in GetRelevantPoints())
    {
        IReadOnlyList<Point> neighbors = GetNeighbors(currentPoint).ToList();
        
        if (currentChar is CellPassage)
        {
            foreach (Point neighborPoint in neighbors.Where(n => CharAt(n) is CellPassage))
            {
                AddTransition(from: currentPoint, to: neighborPoint, levelDelta: 0);
                AddTransition(from: neighborPoint, to: currentPoint, levelDelta: 0);
            }
            
            continue;
        }
        
        Point otherCharPoint = neighbors.Single(p => char.IsLetter(CharAt(p)));
        char otherChar = CharAt(otherCharPoint);
        string pointName = (currentChar < otherChar ? $"{currentChar}{otherChar}" : $"{otherChar}{currentChar}");
                
        if (!namedPoints.ContainsKey(pointName))
        {
            namedPoints[pointName] = [];
        }
        
        namedPoints[pointName].Add(
            GetNeighbors(otherCharPoint).Concat(neighbors).Single(p => CharAt(p) is CellPassage));
    }
    
    foreach (HashSet<Point> points in namedPoints.Values.Where(ps => ps.Count is 2))
    {
        List<Point> pointList = points.ToList();
        Point outerPoint = pointList.Where(IsOuterPoint).Single();
        Point innerPoint = pointList.Where(p => p != outerPoint).Single();
        AddTransition(from: outerPoint, to: innerPoint, levelDelta: -1);
        AddTransition(from: innerPoint, to: outerPoint, levelDelta:  1);
    }
    
    return (
        InitialState: new State(Point: namedPoints[StartName].Single(), Level: 0),
        FinalState: new State(Point: namedPoints[EndName].Single(), Level: 0),
        Transitions: transitions);
        
    IEnumerable<(Point P, char C)> GetRelevantPoints()
    {
        for (int y = 0; y < fieldHeight; y++)
        {
            for (int x = 0; x < fieldWidth; x++)
            {
                var p = new Point(Y: y, X: x);
                char c = CharAt(p);
                
                if (c is not CellPassage && !char.IsLetter(c))
                {
                    continue;
                }
                
                yield return (P: p, C: c);
            }
        }
    }
        
    char CharAt(Point p) => field[p.Y][p.X];
        
    IEnumerable<Point> GetNeighbors(Point p) =>
        Directions.Select(d => p + d).Where(IsValidPoint);
            
    bool IsValidPoint(Point p) =>
        p.Y >= 0 && p.Y < fieldHeight && p.X >= 0 && p.X < fieldWidth;
        
    void AddTransition(Point from, Point to, int levelDelta)
    {
        if (!transitions.ContainsKey(from))
        {
            transitions[from] = [];
        }
        
        transitions[from].Add(new StateTransition(NextPoint: to, LevelDelta: levelDelta));
    }
    
    bool IsOuterPoint(Point p) =>
        p.Y is FieldMargin
        || p.Y == fieldHeight - (FieldMargin + 1)
        || p.X is FieldMargin
        || p.X == fieldWidth - (FieldMargin + 1);
}

private readonly record struct Point(int Y, int X)
{
    public static Point operator +(Point a, Point b) =>
        new Point(Y: a.Y + b.Y, X: a.X + b.X);
}

private readonly record struct State(Point Point, int Level);

private readonly record struct StateTransition(Point NextPoint, int LevelDelta);

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
