<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyDictionary<Point, Node> input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int FeasibilityFactor = 3;

private static long Solve1(IReadOnlyDictionary<Point, Node> input) =>
    input.Sum(kvA =>
        input.Count(kvB =>
            kvB.Key != kvA.Key
            && kvA.Value.Used > 0
            && kvA.Value.Used <= kvB.Value.Avail));

private static long Solve2(IReadOnlyDictionary<Point, Node> input)
{
    Point startPos = input.Keys.Where(p => p.Y == 0).MaxBy(p => p.X)!;
    var finishPos = new Point(X: 0, Y: 0);
    
    char[][] grid = CreateGrid(input, startPos: startPos, finishPos: finishPos);
    
    $"[{nameof(Solve2)}] To better understand the result calculation, please see the map:".Dump();
    grid.Select(l => new string(l)).Dump();
    
    int? leftMostWallTileX = null;
    Point? emptyTilePoint = null;
    
    for (int x = 0; x < grid[0].Length; x++)
    {
        for (int y = 0; y < grid.Length; y++)
        {
            if (grid[y][x] == '#')
            {
                leftMostWallTileX ??= x;
            }
            else if (grid[y][x] == 'O')
            {
                emptyTilePoint ??= new Point(X: x, Y: y);
            }
        }
    }
    
    int distanceAroundWall = emptyTilePoint!.X - leftMostWallTileX!.Value + 1;
    int distanceToLeftFromStart = emptyTilePoint.GetMnDist(startPos) - 1;
    int distanceLeftFromStartToFinish = startPos.X - finishPos.X - 1;
    
    return distanceToLeftFromStart + distanceAroundWall * 2 + distanceLeftFromStartToFinish * 5 + 1;
}

private static char[][] CreateGrid(IReadOnlyDictionary<Point, Node> input, Point startPos, Point finishPos)
{
    int height = input.Keys.Max(p => p.Y) + 1;
    int width = input.Keys.Max(p => p.X) + 1;
    int feasibilityLine = input.Values.Min(n => n.Used + n.Avail) * FeasibilityFactor;
    
    char[][] grid =
        Enumerable.Range(0, height)
            .Select((_, y) =>
                Enumerable.Range(0, width)
                    .Select((_, x) =>
                        input[new Point(x, y)].Used switch
                        {
                            0 => 'O',
                            int s when s < feasibilityLine => 'X',
                            _ => '#'
                        })
                    .ToArray())
            .ToArray();
            
    grid[finishPos.Y][finishPos.X] = 'F';
    grid[startPos.Y][startPos.X] = 'S';
    
    return grid;
}

private static IReadOnlyDictionary<Point, Node> ParseInput(IEnumerable<string> input) =>
    input.Skip(2)
        .Select(nodeLine => nodeLine.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        .ToDictionary(
            nodeLineParts =>
            {
                string[] nodeNameParts = nodeLineParts[0].Split('-');
                
                return new Point(X: int.Parse(nodeNameParts[1][1..]), Y: int.Parse(nodeNameParts[2][1..]));
            },
            nodeLineParts =>
                new Node(Used: int.Parse(nodeLineParts[2][..^1]), Avail: int.Parse(nodeLineParts[3][..^1])));

private record class Point(int X, int Y)
{
    public static Point operator +(Point a, Point b) =>
        new Point(X: a.X + b.X, Y: a.Y + b.Y);
        
    public int GetMnDist(Point other) =>
        Math.Abs(this.X - other.X) + Math.Abs(this.Y - other.Y);
}

private record class Node(int Used, int Avail);

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
