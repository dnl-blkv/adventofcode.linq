<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
    Solve(input).Dump();
}

private const int ImageHeight = 6;
private const int ImageWidth = 25;
private const int LayerSize = ImageHeight * ImageWidth;

private static (long Result1, string Result2) Solve(string input)
{
    IReadOnlyList<string> layers = GetLayers(input).ToArray();
    
    IReadOnlyDictionary<char, int> minZeroCountLayerChars =
        layers.Select(GetCharCounts).MinBy(counts => counts['0'])!;
    int result1 = minZeroCountLayerChars['1'] * minZeroCountLayerChars['2'];
    
    char[][] picture =
        Enumerable.Range(0, ImageHeight)
            .Select(_ => Enumerable.Repeat(' ', ImageWidth).ToArray())
            .ToArray();
    
    for (int i = 0; i < ImageHeight; i++)
    {
        for (int j = 0; j < ImageWidth; j++)
        {
            picture[i][j] =
                layers.Select(l => l[i * ImageWidth + j]).First(c => c is not '2') is '0'
                    ? '.'
                    : '#';
        }
    }
    
    string result2 = string.Join(Environment.NewLine, picture.Select(l => new string(l)));
    
    return (Result1: result1, Result2: result2);
}

private static IEnumerable<string> GetLayers(string input)
{
    int layerCount = input.Length / LayerSize;

    for (int i = 0; i < layerCount; i++)
    {
        int layerStart = i * LayerSize;
        int layerEnd = layerStart + LayerSize;
        yield return input[layerStart..layerEnd];
    }
}

private static IReadOnlyDictionary<char, int> GetCharCounts(string layer)
{
    Dictionary<char, int> result = [];
    
    foreach (char c in layer)
    {
        if (!result.ContainsKey(c))
        {
            result[c] = 0;
        }
    
        result[c]++;
    }

    return result;
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
