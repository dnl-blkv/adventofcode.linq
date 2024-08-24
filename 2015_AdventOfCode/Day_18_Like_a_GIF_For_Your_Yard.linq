<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    char[][] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static int Solve1(char[][] input) =>
    Solve(
        input,
        new HashSet<(int I, int J)>());

private static int Solve2(char[][] input) =>
    Solve(
        input,
        new HashSet<(int I, int J)>
        {
            (I: 0, J: 0),
            (I: 0, J: input[0].Length - 1),
            (I: input.Length - 1, J: 0),
            (I: input.Length - 1, J: input[0].Length - 1)
        });

private static int Solve(char[][] input, HashSet<(int I, int J)> cellsAlwaysOn)
{
    int fieldHeight = input.Length;
    int fieldWidth = input[0].Length;
    
    char[][] field = input.Select(l => l.ToArray()).ToArray();
    char[][] nextField = field.Select(l => l.ToArray()).ToArray();
    
    for (int k = 0; k < 100; k++)
    {
        for (int i = 0; i < fieldHeight; i++)
        {
            for (int j = 0; j < fieldWidth; j++)
            {
                nextField[i][j] =
                    (field[i][j], CountNeighboursOn(i, j), cellsAlwaysOn.Contains((i, j))) switch
                    {
                        ('.', 3, _) or ('#', 2 or 3, _) or (_, _, true) => '#',
                        _ => '.'
                    };
            }
        }
        
        (field, nextField) = (nextField, field);
    }
    
    return field.SelectMany(l => l).Count(c => c == '#');
    
    int CountNeighboursOn(int i, int j)
    {
        int result = 0;
    
        for (int iC = Math.Max(0, i - 1); iC < Math.Min(fieldHeight, i + 2); iC++)
        {
            for (int jC = Math.Max(0, j - 1); jC < Math.Min(fieldWidth, j + 2); jC++)
            {
                if (iC == i && jC == j || field[iC][jC] == '.')
                {
                    continue;
                }
                
                result++;
            }
        }
        
        return result;
    }
}

private static char[][] ParseInput(IEnumerable<string> input) =>
    input.Select(line => line.ToCharArray()).ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}