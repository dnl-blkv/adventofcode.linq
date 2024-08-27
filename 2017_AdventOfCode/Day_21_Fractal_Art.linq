<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyDictionary<string, string[]> input = ParseInput(GetInput());
	Solve(input).Dump();
}

private const char LineSeparator = '/';
private const int IterationCount1 = 5;
private const int IterationCount2 = 18;

private static readonly IReadOnlyList<Func<string[], int, int, char>> NextCharGetters =
[
    (square, i, j) => square[i][j],
    (square, i, j) => square[i][^(j + 1)],
    (square, i, j) => square[^(i + 1)][j],
    (square, i, j) => square[^(i + 1)][^(j + 1)],
    (square, i, j) => square[j][i],
    (square, i, j) => square[j][^(i + 1)],
    (square, i, j) => square[^(j + 1)][i],
    (square, i, j) => square[^(j + 1)][^(i + 1)]
];
private static readonly char[][] InitialPattern =
[
    ['.', '#', '.'],
    ['.', '.', '#'],
    ['#', '#', '#']
];
private static readonly int TotalIterationCount = Math.Max(IterationCount1, IterationCount2);

private static (long Result1, long Result2) Solve(IReadOnlyDictionary<string, string[]> input)
{
    IReadOnlyDictionary<string, string[]> fullArtistsBook = ExpandArtistsBook(input);
    char[][] currentPattern = InitialPattern;
    var results = new List<int>(2);
    
    for (int i = 0; i < TotalIterationCount; i++)
    {
        currentPattern = Enhance(currentPattern, fullArtistsBook);
        
        if (i + 1 is not (IterationCount1 or IterationCount2))
        {
            continue;
        }
        
        results.Add(currentPattern.Sum(row => row.Count(c => c is '#')));
    }
    
    return (Result1: results[0], Result2: results[1]);
}

private static char[][] Enhance(char[][] input, IReadOnlyDictionary<string, string[]> artistsBook)
{
    int inputSize = input.Length;
    int inputBlockSize = (inputSize % 2 == 0) ? 2 : 3;
    int outputBlockSize = inputBlockSize + 1;
    int outputSize = inputSize / inputBlockSize * outputBlockSize;
    char[][] output = Enumerable.Range(0, outputSize).Select(_ => new char[outputSize]).ToArray();
            
    for (int i = 0; i < inputSize / inputBlockSize; i++)
    {
        for (int j = 0; j < inputSize / inputBlockSize; j++)
        {
            IEnumerable<Point> inBlock = EnumerateBlock(i: i, j: j, blockSize: inputBlockSize);
            string inBlockKey = string.Join(string.Empty, inBlock.Select(p => input[p.I][p.J]));
            string[] outBlockValues = artistsBook[inBlockKey];
            int currentOutCell = 0;
            
            foreach (Point p in EnumerateBlock(i: i, j: j, blockSize: outputBlockSize))
            {
                output[p.I][p.J] = outBlockValues[currentOutCell / outputBlockSize][currentOutCell % outputBlockSize];
                currentOutCell++;
            }
        }
    }
    
    return output;
}

private static IEnumerable<Point> EnumerateBlock(int i, int j, int blockSize)
{
    for (int k = 0; k < blockSize; k++)
    {
        for (int l = 0; l < blockSize; l++)
        {
            yield return new Point(I: i * blockSize + k, J: j * blockSize + l);
        }
    }
}

private static IReadOnlyDictionary<string, string[]> ExpandArtistsBook(
    IReadOnlyDictionary<string, string[]> artistsBook)
{
    var result = new Dictionary<string, string[]>();
    
    foreach ((string key, string[] value) in artistsBook)
    {
        foreach (string permutedKey in Permute(key))
        {
            result[permutedKey] = value;
        }
    }
    
    return result;
}

private static IEnumerable<string> Permute(string squareString)
{
    string[] square = squareString.Split(LineSeparator);
    
    return NextCharGetters.Select(getNextChar => string.Join(string.Empty, Walk(square, getNextChar)));
}
    
private static IEnumerable<char> Walk(string[] square, Func<string[], int, int, char> getNextChar)
{
    for (int i = 0; i < square.Length; i++)
    {
        for (int j = 0; j < square[i].Length; j++)
        {
            yield return getNextChar(square, i, j);
        }
    }
}

private static IReadOnlyDictionary<string, string[]> ParseInput(IEnumerable<string> input) =>
    input.Select(l => l.Split(" => ")).ToDictionary(lp => lp[0], lp => lp[1].Split(LineSeparator).ToArray());
    
private readonly record struct Point(int I, int J);

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}