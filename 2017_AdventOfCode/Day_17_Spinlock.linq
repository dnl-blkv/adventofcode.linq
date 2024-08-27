<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private const int IterationCount1 = 2017;
private const int IterationCount2 = 50_000_000;

private static long Solve1(int input)
{
    int nodeCount = 1;
    Node startNode = BuildBuffer(nodeCount);
    Node currentNode = startNode;
    
    for (int i = 1; i <= IterationCount1; i++)
    {
        Node anchorNode = Jump(currentNode, length: input % nodeCount);
        currentNode = InsertAfter(anchorNode, newValue: i);
        nodeCount++;
    }
    
    return currentNode.Next!.Value;
}

private static long Solve2(int input)
{
    int nodeCount = 1;
    int currentNode = 0;
    int result = -1;
    
    for (int i = 1; i <= IterationCount2; i++)
    {
        currentNode = (currentNode + input) % nodeCount + 1;
        nodeCount++;
        
        if (currentNode == 1)
        {
            result = i;
        }
    }
    
    return result;
}

private static Node InsertAfter(Node anchorNode, int newValue)
{
    Node newNode = new Node(newValue);
    (newNode.Prev, newNode.Next, anchorNode.Next) =
        (anchorNode, anchorNode.Next!, (anchorNode.Next!.Prev = newNode));
    
    return newNode;
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

private static Node BuildBuffer(int size)
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

private static string ToString(Node start) => string.Join(' ', GetValues(start));

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

private static int ParseInput(IEnumerable<string> input) => int.Parse(input.Single());
    
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