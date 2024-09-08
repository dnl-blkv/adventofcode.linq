<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int FieldSize = 5;
private const int IterationCount2 = 200;

private static long Solve1(string[] input)
{
    HashSet<string> hashableKeysSeen = [];
    
    return Solve<PlainField, Point>(
        initialField: PlainField.Create(input),
        continueIteration: field => hashableKeysSeen.Add(field.HashableKey),
        getResult: field => field.BiodiversityRating);
}

private static long Solve2(string[] input)
{
    int i = 0;
    
    return Solve<FoldedField, LeveledPoint>(
        initialField: FoldedField.Create(input),
        continueIteration: _ => i++ < IterationCount2,
        getResult: field => field.BugCount);
}

private static long Solve<TField, TPoint>(
    TField initialField,
    Func<TField, bool> continueIteration,
    Func<TField, long> getResult)
    where TField : Field<TField, TPoint>
    where TPoint : notnull
{
    TField currentField = initialField;
    
    while (continueIteration(currentField))
    {
        currentField = currentField.GetNext();
    }

    return getResult(currentField);
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();
    
private class PlainField : Field<PlainField, Point>
{
    private readonly IReadOnlyList<char> allCharsInOrder;

    private PlainField(IReadOnlyDictionary<Point, char> points)
        : base(points)
    {
        this.allCharsInOrder = SingleFieldPoints.Select(this.GetChar).ToList();
        this.HashableKey = string.Join(string.Empty, this.allCharsInOrder);
    }
        
    public string HashableKey { get; }
    
    public long BiodiversityRating
    {
        get 
        {
            long mul = 1;
            
            return this.allCharsInOrder.Sum(c => (c is CellEmpty ? 0 : 1) * (mul *= 2) / 2);
        }
    }
        
    public static PlainField Create(IReadOnlyList<string> input) =>
        new PlainField(points: SingleFieldPoints.ToDictionary(p => p, p => input[p.Y][p.X]));
        
    public override PlainField GetNext() =>
        new PlainField(points: this.Points.Keys.ToDictionary(p => p, this.GetNextCellValue));
        
    protected override IEnumerable<Point> GetNeighbors(Point point) =>
        Directions
            .Select(d => point + d)
            .Where(p => p.Y >= 0 && p.Y < FieldSize && p.X >= 0 && p.X < FieldSize);
}
    
private class FoldedField : Field<FoldedField, LeveledPoint>
{
    private const int MidPos = FieldSize / 2;

    private static readonly IReadOnlyDictionary<Point, Func<int, Point>> InnerLevelNeighborFactories =
        new Dictionary<Point, Func<int, Point>>
        {
            [Directions[0]] = y => new Point(Y: y, X: 0),
            [Directions[1]] = x => new Point(Y: 0, X: x),
            [Directions[2]] = y => new Point(Y: y, X: FieldSize - 1),
            [Directions[3]] = x => new Point(Y: FieldSize - 1, X: x)
        };
        
    private static readonly Point MidPoint =
        new Point(Y: MidPos, X: MidPos);
        
    private static readonly IReadOnlyDictionary<Point, Point> OuterLevelNeighbors =
        Directions.ToDictionary(d => d, d => MidPoint + d);

    private FoldedField(IReadOnlyDictionary<LeveledPoint, char> points)
        : base(points) { }
        
    public int BugCount => this.Points.Values.Count(c => c is CellBug);
        
    public static FoldedField Create(IReadOnlyList<string> input) =>
        new FoldedField(
            points: SingleFieldPoints
                .Where(p => p != MidPoint)
                .ToDictionary(p => new LeveledPoint(Point: p, Level: 0), p => input[p.Y][p.X]));
        
    public override FoldedField GetNext()
    {
        Dictionary<LeveledPoint, char> nextPoints = this.Points.ToDictionary();
        _ = this.Points.Keys.SelectMany(this.GetNeighbors).Select(n => nextPoints.TryAdd(n, CellEmpty)).Count();
        
        foreach (LeveledPoint leveledPoint in nextPoints.Keys)
        {
            nextPoints[leveledPoint] = this.GetNextCellValue(leveledPoint);
        }
        
        return new FoldedField(points: nextPoints);
    }

    protected override IEnumerable<LeveledPoint> GetNeighbors(LeveledPoint leveledPoint) =>
        Directions.SelectMany(direction => ExpandNeighbor(leveledPoint, direction));
    
    private static IEnumerable<LeveledPoint> ExpandNeighbor(LeveledPoint leveledPoint, Point direction)
    {
        (Point point, int level) = leveledPoint;
        
        return (point + direction) switch
        {
            { X: MidPos, Y: MidPos } => GetInnerLevelNeighbors(direction, level),
            Point neighborPoint => Enumerable.Empty<LeveledPoint>().Append(
                neighborPoint is { X: >= 0 and < FieldSize, Y: >= 0 and < FieldSize }
                    ? new LeveledPoint(Point: neighborPoint, Level: level)
                    : GetOuterLevelNeighbor(direction, level))
        };
    }
                
    private static IEnumerable<LeveledPoint> GetInnerLevelNeighbors(Point direction, int level) =>
        Enumerable.Range(0, FieldSize).Select(c =>
            new LeveledPoint(Point: InnerLevelNeighborFactories[direction].Invoke(c), Level: level + 1));
        
    private static LeveledPoint GetOuterLevelNeighbor(Point direction, int level) =>
        new LeveledPoint(Point: OuterLevelNeighbors[direction], Level: level - 1);
}
    
private abstract class Field<TField, TPoint>
    where TField : Field<TField, TPoint>
    where TPoint : notnull
{
    protected const char CellEmpty = '.';
    protected const char CellBug = '#';

    protected static readonly IReadOnlyList<Point> Directions =
        [
            new Point(Y:  0, X:  1),
            new Point(Y:  1, X:  0),
            new Point(Y:  0, X: -1),
            new Point(Y: -1, X:  0)
        ];
        
    protected static readonly IReadOnlyList<Point> SingleFieldPoints =
        Enumerable.Range(0, FieldSize)
            .SelectMany(y => Enumerable.Range(0, FieldSize).Select(x =>new Point(Y: y, X: x)))
            .ToList();
        
    public abstract TField GetNext();

    protected Field(IReadOnlyDictionary<TPoint, char> points) =>
        this.Points = points ?? throw new ArgumentNullException(nameof(points));
        
    protected IReadOnlyDictionary<TPoint, char> Points { get; }

    protected char GetNextCellValue(TPoint point) =>
        (GetChar(point), this.GetNeighbors(point).Count(n => this.GetChar(n) is CellBug)) switch
        {
            (CellEmpty, 1 or 2) => CellBug,
            (CellBug, not 1) => CellEmpty,
            (char c, _) => c
        };
    
    protected char GetChar(TPoint point) =>
        this.Points.TryGetValue(point, out char c)
            ? c
            : CellEmpty;

    protected abstract IEnumerable<TPoint> GetNeighbors(TPoint point);
        
    private int CountBugNeighbors(TPoint point) =>
        this.GetNeighbors(point).Count(n => this.GetChar(n) is CellBug);
}

private readonly record struct Point(int Y, int X)
{
    public static Point operator +(Point a, Point b) =>
        new Point(Y: a.Y + b.Y, X: a.X + b.X);
}

private readonly record struct LeveledPoint(Point Point, int Level);

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
