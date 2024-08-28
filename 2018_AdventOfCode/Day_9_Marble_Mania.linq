<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (int playerCount, int lastMarble) = ParseInput(GetInput());
    Solve1(playerCount, lastMarble).Dump();
    Solve2(playerCount, lastMarble).Dump();
}

private const int InsertShift = 1;
private const int RemovePeriod = 23;
private const int RemoveShift = -7;
private const int MarbleMultiplier2 = 100;

private static long Solve1(int playerCount, int lastMarble) =>
    Solve(playerCount, lastMarble);

private static long Solve2(int playerCount, int lastMarble) =>
    Solve(playerCount, lastMarble * MarbleMultiplier2);

private static long Solve(int playerCount, int lastMarble)
{
    long[] scores = Enumerable.Repeat(0L, playerCount).ToArray();
    Node startNode = Node.CreateUnicircle();
    
    for (int m = 0; m <= lastMarble; m++)
    {
        if (m % RemovePeriod is not 0)
        {
            startNode = startNode.InsertAfter(shift: InsertShift, value: m);
            continue;
        }
        
        Node nodeToRemove = startNode.Move(RemoveShift);
        scores[m % playerCount] += m + nodeToRemove.Value;
        startNode = nodeToRemove.Remove();
    }
    
    return scores.Max();
}

private static (int PlayerCount, int LastMarble) ParseInput(IEnumerable<string> input)
{
    string line = input.Single();
    string[] lineParts = line.Split(' ');

    return (PlayerCount: int.Parse(lineParts[0]), LastMarble: int.Parse(lineParts[^2]));
}

private class Node(long value)
{
    private static IReadOnlyDictionary<bool, Func<Node, Node>> MoveTable =
        new Dictionary<bool, Func<Node, Node>>
        {
            [true] = node => node.Next!,
            [false] = node => node.Prev!
        };

    public long Value { get; } = value;

    public Node? Next { get; private set; }
    
    public Node? Prev { get; private set; }
    
    public static Node CreateUnicircle()
    {
        var node = new Node(value: 0);
        
        return node.Prev = node.Next = node;
    }
    
    public Node InsertAfter(int shift, long value)
    {
        Node anchorNode = this.Move(shift);
        var newNode = new Node(value)
        {
            Prev = anchorNode,
            Next = anchorNode.Next!
        };
        
        return anchorNode.Next = (anchorNode.Next!.Prev = newNode);
    }
    
    public Node Move(int dist)
    {
        Node currentNode = this;
        Func<Node, Node> moveOnce = MoveTable[dist > 0];
        
        for (int i = 0; i < Math.Abs(dist); i++)
        {
            currentNode = moveOnce(currentNode);
        }
        
        return currentNode;
    }
    
    public Node Remove()
    {
        Node leftNode = this.Prev!;
        Node rightNode = this.Next!;
        leftNode.Next = rightNode;
        rightNode.Prev = leftNode;
        
        return rightNode;
    }
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
