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

private static readonly IReadOnlyList<Point> Directions =
[
    new Point(I:  0, J:  1),
    new Point(I: -1, J:  0),
    new Point(I:  0, J: -1),
    new Point(I:  1, J:  0)
];

private static readonly IReadOnlyList<Point> Neighbors =
    Directions.Concat(
        [
            new Point(I: -1, J: -1),
            new Point(I:  1, J: -1),
            new Point(I:  1, J:  1),
            new Point(I: -1, J:  1)
        ])
        .ToArray();

private static long Solve1(int input)
{
    int ring = (int)Math.Ceiling(Math.Sqrt(input)) / 2;
    int side = ring * 2 + 1;
    int innerSide = side - 2;
    int innerArea = innerSide * innerSide;
    int perimeter = Math.Max(1, (innerSide + 1) * 4);
    int posInSide = (input - innerArea) % perimeter;
    int distToSideMid = Math.Abs((posInSide % (Math.Max(1, side - 1))) - side / 2);
    
    return ring + distToSideMid;
}

private static long Solve2(long input)
{
    int ring = (int)Math.Ceiling(Math.Sqrt(input + 1)) / 2;
    int side = ring * 2 + 1;
    int[][] grid = Enumerable.Range(0, side).Select(_ => Enumerable.Repeat(0, side).ToArray()).ToArray();
    var pos = new Point(I: side / 2, J: side / 2);
    int direction = 0;
    int currentSide = 1;
    int sideRem = currentSide;
    grid[pos.I][pos.J] = 1;
    
    while (grid[pos.I][pos.J] <= input)
    {
        pos += Directions[direction];
        grid[pos.I][pos.J] = Neighbors.Select(nD => pos + nD).Sum(n => grid[n.I][n.J]);

        if (--sideRem > 0)
        {
            continue;
        }
        
        direction = (direction + 1) % Directions.Count;
        
        if (direction == 1)
        {
            currentSide += 2;
        }
        
        sideRem = currentSide - direction switch
        {
            0 => 0,
            1 => 2,
            _ => 1
        };
    }
    
    return grid[pos.I][pos.J];
}

private static int ParseInput(IEnumerable<string> input) => int.Parse(input.Single());

private record class Point(int I, int J)
{
    public static Point operator +(Point a, Point b) =>
        new Point(I: a.I + b.I, J: a.J + b.J);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
