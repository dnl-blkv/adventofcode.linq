<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int Pad1Height = 3;
private const int Pad1Width = 3;
private const int Pad2Radius = 2;
private const int HexBase = 16;

private static readonly IReadOnlyDictionary<char, (int Di, int Dj)> MoveTable =
    new Dictionary<char, (int Di, int Dj)>
    {
        ['U'] = (Di: -1, Dj:  0),
        ['R'] = (Di:  0, Dj:  1),
        ['D'] = (Di:  1, Dj:  0),
        ['L'] = (Di:  0, Dj: -1),
    };

private static string Solve1(string[] input) =>
    Solve(
        input,
        startPos: (I: 1, J: 1),
        isValidPos: (i, j) => i is (>= 0 and < Pad1Height) && j is (>= 0 and < Pad1Width),
        getDigitValue: (i, j) => i * Pad1Width + j + 1);

private static string Solve2(string[] input) =>
    Solve(
        input,
        startPos: (I: 0, J: -2),
        isValidPos: (i, j) => Math.Abs(i) + Math.Abs(j) <= Pad2Radius,
        getDigitValue: (i, j) =>
            (Pad2Radius + i) * (Pad2Radius + i)
            + 2 * (Pad2Radius - i * i) * (i > 1 ? 1 : 0)
            + (Pad2Radius + j - Math.Abs(i))
            + 1);

private static string Solve(
    string[] input,
    (int I, int J) startPos,
    Func<int, int, bool> isValidPos,
    Func<int, int, int> getDigitValue)
{
    (int I, int J) curPos = startPos;
    long code = 0;
    
    foreach (string moves in input)
    {
        curPos = moves.Aggregate(
            curPos,
            (pos, d) =>
                MoveTable[d] is (int dI, int dJ)
                && pos.I + dI is int iC && pos.J + dJ is int jC
                && isValidPos(iC, jC)
                    ? (iC, jC)
                    : pos);
        code = code * HexBase + getDigitValue(curPos.I, curPos.J);
    }
    
    return Convert.ToString(code, HexBase).ToUpper();
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
