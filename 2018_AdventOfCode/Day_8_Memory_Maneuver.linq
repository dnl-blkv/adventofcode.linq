<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int[] input = ParseInput(GetInput());
    Solve(input).Dump();
}

private static (long Resul1, long Result2) Solve(int[] input)
{
    Node root = Node.Parse(input).Node;
    
    return (Resul1: root.MetadataTotal, Result2: root.Value);
}

private static int[] ParseInput(IEnumerable<string> input) =>
    input.Single().Split(' ').Select(int.Parse).ToArray();
    
private class Node(IReadOnlyList<Node> children, IReadOnlyList<int> metadata)
{
    public IReadOnlyList<Node> Children { get; } = children;
    
    public IReadOnlyList<int> Metadata { get; } = metadata;
    
    public int MetadataTotal =>
        this.Metadata.Concat(this.Children.Select(c => c.MetadataTotal)).Sum();
    
    public int Value => 
        this.Children.Count > 0
            ? this.Metadata.Select(m => (m <= this.Children.Count ? this.Children[m - 1].Value : 0)).Sum()
            : this.Metadata.Sum();
    
    public static (Node Node, int NextStart) Parse(ReadOnlySpan<int> input, int start = 0)
    {
        int childrenCount = input[start];
        int metadataLength = input[start + 1];
        int cursor = start + 2;
        List<Node> children = [];
        
        for (int c = 0; c < childrenCount; c++)
        {
            (Node child, int nextStart) = Parse(input, cursor);
            children.Add(child);
            cursor = nextStart;
        }
        
        int nextNodeStart = cursor + metadataLength;
        
        return (
            Node: new Node(children, metadata: input[cursor..nextNodeStart].ToArray()),
            NextStart: nextNodeStart);
    }
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
