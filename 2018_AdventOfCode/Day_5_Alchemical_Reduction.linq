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

private const int SmallToCapitalDistance = 32;

private static long Solve1(string input) =>
    Solve(chainChars: input);

private static long Solve2(string input) => 
    input.Select(char.ToLower).Distinct().Min(ec => Solve(input.Where(c => char.ToLower(c) != ec)));

private static long Solve(IEnumerable<char> chainChars)
{
    Node? firstNode = BuildChain(chainChars);
    bool reducedSome;
    Node? currentNode;
    
    do
    {
        reducedSome = false;
        currentNode = firstNode;
        
        while (currentNode?.Next is not null)
        {
            if (Math.Abs(currentNode.Next.Value - currentNode.Value) is not SmallToCapitalDistance)
            {
                currentNode = currentNode.Next;
                continue;
            }
            
            Node? afterNext = currentNode.Next.Next;
            
            if (afterNext is not null)
            {
                afterNext.Prev = currentNode.Prev;
            }
            
            Node? prev = currentNode.Prev;
            
            if (prev is not null)
            {
                prev.Next = afterNext;
            }

            currentNode = prev ?? afterNext;
            
            if (currentNode?.Prev is null)
            {
                firstNode = currentNode;
            }
            
            reducedSome = true;
        }
    } while (reducedSome);
    
    int result = 0;
    currentNode = firstNode;
    
    while (currentNode is not null)
    {
        result++;
        currentNode = currentNode.Next;
    }
    
    return result;
}

private static Node? BuildChain(IEnumerable<char> input)
{
    Node? firstNode = null;
    Node? currentNode = null;
    
    foreach (char c in input)
    {
        var nextNode = new Node(c);
        
        if (currentNode is null)
        {
            firstNode = currentNode = nextNode;
            continue;
        }
        
        (nextNode.Prev, currentNode.Next) = (currentNode, nextNode);
        currentNode = nextNode;
    }
    
    return firstNode;
}

private static string ParseInput(IEnumerable<string> input) => input.Single();
    
private class Node(char value)
{
    public char Value { get; } = value;
    
    public Node? Next { get; set; }
    
    public Node? Prev { get; set; }
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
