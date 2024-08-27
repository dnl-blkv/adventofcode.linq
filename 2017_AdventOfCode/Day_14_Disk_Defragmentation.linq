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

private const int GridRowCount = 128;
private const int HexBase = 16;
private const int BinBase = 2;

private const int KnotHashRoundCount = 64;
private const int RopeLength = 256;
private const int SparseHashSize = 256;
private const int DenseHashBlockSize = 16;

private static readonly int BinDigitsPerHexDigit = (int)Math.Log(HexBase, BinBase);
private static readonly int[] KnotHashSuffix = [17, 31, 73, 47, 23];
private static readonly IReadOnlyList<Point> Neighbors =
[
    new Point(I:  0, J:  1),
    new Point(I:  1, J:  0),
    new Point(I:  0, J: -1),
    new Point(I: -1, J:  0)
];

private static (long Result1, long Result2) Solve(string input)
{
    bool[][] grid =
        GetGridHashes(input)
            .Select(l => l.ToCharArray().Select(c => c == '1').ToArray())
            .ToArray();
    HashSet<Point> usedCells = [];
    
    for (int i = 0; i < grid.Length; i++)
    {
        for (int j = 0; j < grid[i].Length; j++)
        {
            if (!grid[i][j])
            {
                continue;
            }
            
            usedCells.Add(new Point(I: i, J: j));
        }
    }
    
    return (Result1: usedCells.Count, Result2: CountRegions(usedCells));
}

private static IEnumerable<string> GetGridHashes(string input)
{
    for (int i = 0; i < GridRowCount; i++)
    {
        yield return string.Join(
            string.Empty,
            GetKnotHash(input: $"{input}-{i}").Select(d =>
                Convert.ToString(Convert.ToInt32($"{d}", HexBase), BinBase)
                    .PadLeft(BinDigitsPerHexDigit, '0')));
    }
}

private static string GetKnotHash(string input)
{
    int[] lengths = input.Select(c => (int)c).Concat(KnotHashSuffix).ToArray();
    Node first = RunKnotHash(lengths, KnotHashRoundCount);
    int[] sparseHash = GetValues(first).ToArray();
    int[] denseHash = CreateDenseHash(sparseHash).ToArray();
    
    return string.Join(string.Empty, denseHash.Select(n => $"{n:X2}"));
}

private static Node RunKnotHash(int[] lengths, int roundCount)
{
    RunState? lastState = null;
    
    for (int i = 0; i < roundCount; i++)
    {
        lastState = RunKnotHashRound(lengths, lastState);
    }
    
    return lastState!.GetFirstNode();
}

private static RunState RunKnotHashRound(int[] lengths, RunState? lastState = null)
{
    lastState ??= new RunState(Current: BuildRope(RopeLength), SkipSize: 0, TotalSkip: 0);
    (Node current, int skipSize, int totalSkip) = lastState;
    
    foreach (int length in lengths)
    {
        current = Twist(current, length: length, skipSize: skipSize);
        totalSkip += length + skipSize;
        skipSize++;
    }
    
    return new RunState(current, SkipSize: skipSize, TotalSkip: totalSkip);
}

private static Node BuildRope(int size)
{
    Node start = new Node(value: 0);
    Node? current = start;
    
    for (int i = 1; i < size; i++)
    {
        Node next = new Node(i)
        {
            Prev = current
        };
        current.Next = next;
        current = next;
    }
    
    current!.Next = start;
    start.Prev = current;
    
    return start;
}

private static Node Twist(Node start, int length, int skipSize)
{
    Node left = start;
    Node preLeft = left.Prev!;
    Node right = Jump(start, length - 1);
    Node postRight = right.Next!;
    
    left.Prev = null;
    preLeft.Next = null;
    right.Next = null;
    postRight.Prev = null;
    
    Reverse(start: left, end: right);
    
    if (preLeft == right)
    {
        left.Next = right;
        right.Prev = left;
    }
    else
    {
        preLeft.Next = right;
        right.Prev = preLeft;
        left.Next = postRight;
        postRight.Prev = left;
    }
    
    return Jump(right, length: length + skipSize);
}

private static Node Jump(Node start, int length)
{
    Node result = start;

    for (int i = 0; i < length; i++)
    {
        result = result.Next!;
    }
    
    return result;
}

private static void Reverse(Node start, Node end)
{
    Node? prev = null;
    Node current = start;
    
    while (current is not null)
    {
        Node next = current.Next!;
        current.Prev = next;
        current.Next = prev;
        prev = current;
        current = next;
    }
}

private static IEnumerable<int> GetValues(Node start)
{
    Node current = start;
    
    do
    {
        yield return current.Value;
        current = current.Next!;
    }
    while (current != start && current is not null);
}

private static IEnumerable<int> CreateDenseHash(int[] sparseHash)
{
    if (sparseHash.Length != SparseHashSize)
    {
        foreach (int term in sparseHash)
        {
            yield return term;
        };
        
        yield break;
    }
    
    for (int i = 0; i < SparseHashSize; i += DenseHashBlockSize)
    {
        int term = sparseHash[i];
        
        for (int j = 1; j < DenseHashBlockSize; j++)
        {
            term ^= sparseHash[i + j];
        }
        
        yield return term;
    }
}

private static int CountRegions(IEnumerable<Point> emptyCells)
{
    int result = 0;
    var unvisited = new HashSet<Point>(emptyCells);
    
    while (unvisited.Count > 0)
    {
        Point initialState = unvisited.First();
        unvisited.Remove(initialState);
        HashSet<Point> currentStates = [initialState];
        
        while (currentStates.Count > 0)
        {
            currentStates =
                currentStates
                    .SelectMany(s => Neighbors.Select(n => s + n).Where(unvisited.Remove))
                    .ToHashSet();
        }
        
        result++;
    }
    
    return result;
}

private static string ToString(Node start) => string.Join(' ', GetValues(start));

private static string ParseInput(IEnumerable<string> input) => input.Single();

private record struct Point(int I, int J)
{
    public static Point operator +(Point a, Point b) =>
        new Point(I: a.I + b.I, J: a.J + b.J);
}

private record RunState(Node Current, int SkipSize, int TotalSkip)
{
    public Node GetFirstNode() =>
        Jump(this.Current, length: RopeLength - this.TotalSkip % RopeLength);
}
    
private class Node(int value)
{
    public int Value { get; } = value;

    public Node? Next { get; set; }
    
    public Node? Prev { get; set; }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}