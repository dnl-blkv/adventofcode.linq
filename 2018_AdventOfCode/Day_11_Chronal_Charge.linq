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

private const int GridSize = 300;

private const int MinBlockSize1 = 3;
private const int MaxBlockSize1 = MinBlockSize1;
private const int AnswerTerms1 = 2;

private const int MinBlockSize2 = 1;
private const int MaxBlockSize2 = GridSize;
private const int AnswerTerms2 = 3;

private const char AnswerSeparator = ',';

private static string Solve1(int input) =>
    FormatAnswer(
        result: Solve(input, minBlockSize: MinBlockSize1, maxBlockSize: MaxBlockSize1),
        answerTerms: AnswerTerms1);

private static string Solve2(int input) =>
    FormatAnswer(
        result: Solve(input, minBlockSize: MinBlockSize2, maxBlockSize: MaxBlockSize2),
        answerTerms: AnswerTerms2);

private static int[] Solve(int gridSerialNumber, int minBlockSize, int maxBlockSize)
{
    int[][] grid = InitEmptyGrid();
    
    for (int i = 0; i < GridSize; i++)
    {
        for (int j = 0; j < GridSize; j++)
        {
            grid[i][j] = CalculateCellEnergy(x: j + 1, y: i + 1);
        }
    }
    
    int[][] rowSums = InitEmptyGrid();
    int[][] colSums = InitEmptyGrid();
    
    for (int k = 0; k < GridSize; k++)
    {
        int r = 0;
        int c = 0; 
        
        for (int l = 0; l < GridSize; l++)
        {
            rowSums[k][l] = (r += grid[k][l]);
            colSums[k][l] = (c += grid[l][k]);
        }
    }
    
    var result = new int[3];
    int maxBlockEnergy = int.MinValue;
    
    for (int bY = 0; bY < GridSize; bY++)
    {
        for (int bX = 0; bX < GridSize; bX++)
        {
            int currentMaxBlockSize = Math.Min(Math.Min(GridSize - bY, GridSize - bX), maxBlockSize);
            int currentBlockEnergy = 0;
            
            for (int blockSize = 1; blockSize <= currentMaxBlockSize; blockSize++)
            {                
                int bottomY = bY + blockSize - 1;
                int rightX = bX + blockSize - 1;
                int bottomRowSum = GetRowSum(y: bottomY, from: bX, to: rightX);
                int rightColSum = GetColSum(x: rightX, from: bY, to: bottomY);
                currentBlockEnergy += bottomRowSum + rightColSum - grid[bottomY][rightX];
                
                if (blockSize < minBlockSize || currentBlockEnergy <= maxBlockEnergy)
                {
                    continue;
                }
                
                result[0] = bX + 1;
                result[1] = bY + 1;
                result[2] = blockSize;
                maxBlockEnergy = currentBlockEnergy;
            }
        }
    }
    
    return result;
    
    static int[][] InitEmptyGrid() =>
        Enumerable.Range(0, GridSize).Select(_ => new int[GridSize]).ToArray();
        
    static int GetRangeSum(int[][] sums, int k, int from, int to) =>
        sums[k][to] - (from < 1 ? 0 : sums[k][from - 1]);
        
    int CalculateCellEnergy(int x, int y)
    {
        int rackId = x + 10;
        
        return (((y * rackId + gridSerialNumber) * rackId) / 100) % 10 - 5;
    }
    
    int GetRowSum(int y, int from, int to) =>
        GetRangeSum(rowSums, k: y, from: from, to: to);
    
    int GetColSum(int x, int from, int to) =>
        GetRangeSum(colSums, k: x, from: from, to: to);
}

private static string FormatAnswer(int[] result, int answerTerms) =>
    string.Join(AnswerSeparator, result.Take(answerTerms));

private static int ParseInput(IEnumerable<string> input) => int.Parse(input.Single());

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}