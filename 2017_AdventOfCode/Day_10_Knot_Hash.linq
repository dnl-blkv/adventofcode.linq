<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int RopeLength = 256;

private const int RoundCount1 = 1;
private const int RoundCount2 = 64;

private const int SparseHashSize = 256;
private const int DenseHashBlockSize = 16;

private static readonly int[] Part2Suffix = [17, 31, 73, 47, 23];

private static long Solve1(string input)
{
    int[] lengths = input.Split(',').Select(s => int.Parse(s.Trim())).ToArray();
    Node first = Run(lengths, RoundCount1);
    
    return first.Value * first.Next!.Value;
}

private static string Solve2(string input)
{
    int[] lengths = input.Select(c => (int)c).Concat(Part2Suffix).ToArray();
    Node first = Run(lengths, RoundCount2);
    int[] sparseHash = GetValues(first).ToArray();
    int[] denseHash = CreateDenseHash(sparseHash).ToArray();
    
    return string.Join(string.Empty, denseHash.Select(n => $"{n:X2}"));
}

private static Node Run(int[] lengths, int roundCount)
{
    RunState? lastState = null;
    
    for (int i = 0; i < roundCount; i++)
    {
        lastState = RunRound(lengths, lastState);
    }
    
    return lastState!.GetFirstNode();
}

private static RunState RunRound(int[] lengths, RunState? lastState = null)
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
    var start = new Node(value: 0);
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

private static string ToString(Node start) => string.Join(' ', GetValues(start));

private static string ParseInput(IEnumerable<string> input) => input.Single();

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
