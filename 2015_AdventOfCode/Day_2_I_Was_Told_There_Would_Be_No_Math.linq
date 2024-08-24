<Query Kind="Program" />

void Main()
{
    (int l, int w, int h)[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static int Solve1((int l, int w, int h)[] input) =>
    input
        .Select(d => (s1: d.l * d.w, s2: d.l * d.h, s3: d.w * d.h))
        .Select(ss => ss.s1 * 2 + ss.s2 * 2 + ss.s3 * 2 + Math.Min(ss.s1, Math.Min(ss.s2, ss.s3)))
        .Sum();

private static int Solve2((int l, int w, int h)[] input) =>
    input
        .Select(d => (p1: (d.l + d.w) * 2, p2: (d.l + d.h) * 2, p3: (d.w + d.h) * 2, v: d.l * d.w * d.h))
        .Select(pv => Math.Min(pv.p1, Math.Min(pv.p2, pv.p3)) + pv.v)
        .Sum();
        
private static (int l, int w, int h)[] ParseInput(IEnumerable<string> input) =>
    input
        .Select(line =>
        {
            int[] dimensions = line.Split("x").Select(int.Parse).ToArray();
            return (l: dimensions[0], w: dimensions[1], h: dimensions[2]);
        })
        .ToArray();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}