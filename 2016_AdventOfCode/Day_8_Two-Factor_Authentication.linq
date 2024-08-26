<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Action<char[][]>[] input = ParseInput(GetInput());
	Solve(input).Dump();
}

private const int ScreenHeight = 6;
private const int ScreenWidth = 50;

private static (long Result1, string Result2) Solve(Action<char[][]>[] input)
{
    char[][] field =    
        Enumerable.Range(0, ScreenHeight)
            .Select(_ => Enumerable.Range(0, ScreenWidth).Select(_ => '.').ToArray())
            .ToArray();

    foreach (Action<char[][]> command in input)
    {
        command.Invoke(field);
    }
    
    var result2Builder = new StringBuilder();
    
    foreach (char[] line in field)
    {
        result2Builder.AppendLine(string.Join(string.Empty, line));
    }
    
    return (Result1: field.SelectMany(c => c).Count(c => c is '#'), Result2: result2Builder.ToString());
}

private static Action<char[][]>[] ParseInput(IEnumerable<string> input) =>
    input
        .Select<string, Action<char[][]>>(instructionString =>
        {
            string[] instructionStringParts = instructionString.Split(' ', 2);
            string type = instructionStringParts[0];
            string[] args = instructionStringParts[1].Split(' ');
            
            switch (type)
            {
                case "rect":
                    int[] size = args[0].Split('x').Select(int.Parse).ToArray();
                
                    return field =>
                    {
                        for (int i = 0; i < size[1]; i++)
                        {
                            for (int j = 0; j < size[0]; j++)
                            {
                                field[i][j] = '#';
                            }
                        }
                    };
                    
                case "rotate":
                    int index = int.Parse(args[1].Split('=')[1]);
                    int n = int.Parse(args[3]);
                    
                    return args[0] switch
                    {
                        "row" =>
                            field => NRotate(
                                size: field[index].Length,
                                get: j => field[index][j],
                                set: (j, value) => field[index][j] = value,
                                n: n),
                        "column" =>
                            field => NRotate(
                                size: field.Length,
                                get: i => field[i][index],
                                set: (i, value) => field[i][index] = value,
                                n: n),
                        _ => throw new ArgumentException($"Unexpected '{type}' instruction subtype: '{args[0]}'")
                    };
                            
                default:
                    throw new ArgumentException($"Unexpected instruction type: '{type}'");
            }
        })
        .ToArray();

private static void NRotate<T>(int size, Func<int, T> get, Action<int, T> set, int n)
{
    int nCorrected = (size + (n % size)) % size;

    for (int s = 0; s < GetGreatestCommonDivisor(size, nCorrected); s++)
    {
        int current = s;
    
        do
        {
            current = (current + nCorrected + size) % size;
            T sValue = get(s);
            set(s, get(current));
            set(current, sValue);
            
        } while (current != s);
    }
}

private static long GetGreatestCommonDivisor(params long[] vals)
{
    var curVals = new long[vals.Length];
    vals.CopyTo(curVals, 0);
    long result = 1;
    
    for (long div = 2; div <= vals.Min(); div++)
    {
        while (curVals.All(v => v % div == 0))
        {
            for (int i = 0; i < curVals.Length; i++)
            {
                curVals[i] /= div;
            }
            
            result *= div;
        }
    }
    
    return result;
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}