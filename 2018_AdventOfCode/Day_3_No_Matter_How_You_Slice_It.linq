<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Claim[] input = ParseInput(GetInput());
	Solve(input).Dump();
}

private static (long Result1, long Result2) Solve(Claim[] input)
{
    Dictionary<Point, List<int>> fabric = [];
    
    foreach (((int left, int top, int width, int height), int claimNo) in input.Select((c, n) => (c, n)))
    {
        int bottom = top + height - 1;
        int right = left + width - 1;
    
        for (int i = top; i <= bottom; i++)
        {
            for (int j = left; j <= right; j++)
            {
                var pos = new Point(I: i, J: j);
                
                if (!fabric.ContainsKey(pos))
                {
                    fabric[pos] = [];
                }
                
                fabric[pos].Add(claimNo);
            }
        }
    }
    
    HashSet<int> overlapping = [];
    
    foreach ((Point _, IReadOnlyList<int> claims) in fabric.Where(kv => kv.Value.Count > 1))
    {
        overlapping.UnionWith(claims);
    }
    
    return (
        Result1: fabric.Count(kv => kv.Value.Count >= 2),
        Result2: Enumerable.Range(0, input.Length).Except(overlapping).Single() + 1);
}

private static Claim[] ParseInput(IEnumerable<string> input) =>
    input.Select(Claim.Parse).ToArray();

private record struct Claim(int Left, int Top, int Width, int Height)
{
    public static Claim Parse(string claimLine)
    {
        string[] claimLineParts = claimLine[(claimLine.IndexOf('@') + 2)..].Split(": ");
        int[] offsets = claimLineParts[0].Split(',').Select(int.Parse).ToArray();
        int[] dimensions = claimLineParts[1].Split('x').Select(int.Parse).ToArray();
        
        return new Claim(Left: offsets[0], Top: offsets[1], Width: dimensions[0], Height: dimensions[1]);
    }
}

private record struct Point(int I, int J);

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}