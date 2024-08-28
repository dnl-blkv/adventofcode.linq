<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (int row, int column) = ParseInput(GetInput());
    Solve(row, column).Dump();
}

private static long Solve(int row, int column)
{
    int diagonalNumber = row + column - 1;
    int index = diagonalNumber * (diagonalNumber - 1) / 2 + column;
    long result = 20151125;
    
    for (int i = 0; i < index - 1; i++)
    {
        result = (result * 252533) % 33554393;
    }
    
    return result;
}

private static (int Row, int Column) ParseInput(IEnumerable<string> input)
{
    string[] lineParts = input.Single().Split(' ');
    
    return (
        Row: int.Parse(lineParts[^3].Trim(',')),
        Column: int.Parse(lineParts[^1].Trim('.')));
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
