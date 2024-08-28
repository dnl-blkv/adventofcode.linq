<Query Kind="Program">
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<Operation> input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private enum OperationType
{
    Off = 0,
    On = 1,
    Toggle = 2
}

private const int BoardSize = 1000;

private static long Solve1(IReadOnlyList<Operation> input) =>
    Solve(
        input,
        _ => false,
        (operation, bulb) => operation.Type switch
        {
            OperationType.Toggle => !bulb,
            OperationType.On => true,
            OperationType.Off => false,
            _ => throw new Exception("Should never happen!")
        },
        board => board.SelectMany(x => x).Count(b => b));

private static long Solve2(IReadOnlyList<Operation> input) =>
    Solve(
        input,
        _ => 0L,
        (operation, bulb) => operation.Type switch
        {
            OperationType.Toggle => bulb + 2L,
            OperationType.On => bulb + 1L,
            OperationType.Off => Math.Max(0L, bulb - 1L),
            _ => throw new Exception("Should never happen!")
        },
        board => board.SelectMany(x => x).Sum());

private static long Solve<TBulb>(
    IReadOnlyList<Operation> input,
    Func<int, TBulb> createBulb,
    Func<Operation, TBulb, TBulb> updateBulb,
    Func<TBulb[][], long> calculateResult)
{
    TBulb[][] board = CreateBoard(BoardSize, createBulb);
    
    foreach (Operation operation in input)
    {
        for (int i = operation.From.X; i <= operation.To.X; i++)
        {
            for (int j = operation.From.Y; j <= operation.To.Y; j++)
            {
                board[i][j] = updateBulb(operation, board[i][j]);
            }
        }
    }
    
    return calculateResult(board);
}

private static TBulb[][] CreateBoard<TBulb>(int size, Func<int, TBulb> createBulb) =>
    Enumerable.Range(0, size)
        .Select(_ => Enumerable.Range(0, size).Select(createBulb).ToArray())
        .ToArray();
        
private static IReadOnlyList<Operation> ParseInput(IEnumerable<string> input) =>
    input.Select(Operation.Parse).ToArray();

private class Operation
{
    public OperationType Type { get; private set; }
    
    public (int X, int Y) From { get; private set; }
    
    public (int X, int Y) To { get; private set; }
    
    public static Operation Parse(string operationString)
    {
        string[] operationStringParts = operationString.Split(' ');
        
        return operationStringParts[0] == "toggle"
            ? new Operation
            {
                Type = OperationType.Toggle,
                From = ParsePoint(operationStringParts[1]),
                To = ParsePoint(operationStringParts[3])
            }
            : new Operation
            {
                Type = operationStringParts[1] == "on"
                    ? OperationType.On
                    : OperationType.Off,
                From = ParsePoint(operationStringParts[2]),
                To = ParsePoint(operationStringParts[4])
            };
    }
    
    private static (int X, int Y) ParsePoint(string pointString)
    {
        string[] pointStringParts = pointString.Split(',');
        
        return (X: int.Parse(pointStringParts[0]), Y: int.Parse(pointStringParts[1]));
    }
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
