<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Action<Program>[] input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2().Dump();
}

private const int RangeStart2 = 108_400;
private const int StepCount2 = 1_000;
private const int Step2 = 17;

private static readonly List<long> PrimeList = [2, 3];
private static readonly HashSet<long> PrimeSet = PrimeList.ToHashSet();

private static long Solve1(Action<Program>[] input)
{
    var p = new Program();
    
    while (p.LastInstruction >= -1 && p.LastInstruction < input.Length - 1)
    {
        input[++p.LastInstruction](p);
    }
    
    return p.MulCount;
}

/***
Original code translated to C#:

private static long Solve2Unoptimal()
{
    int a = 1, b = 0, c = 0, d = 0, e = 0, f = 0, g = 0, h = 0;
    
    b = 84;
    c = b;

    if (a != 0)
    {
        b *= 100;
        b += 100000;
        c = b;
        c += 17000;
    }

    while (true)
    {
        f = 1;
        d = 2;

        do
        {
            e = 2;

            do
            {
                g = d;
                g *= e;
                g -= b;

                if (g == 0)
                {
                    f = 0;
                }

                e += 1;
                g = e;
                g -= b;
            } while (g != 0);

            d += 1;
            g = d;
            g -= b;
        } while (g != 0);

        if (f == 0)
        {
            h += 1;
        }

        g = b;
        g -= c;

        if (g == 0)
        {
            return h;
        }

        b += 17;
    }
}
***/
private static long Solve2() =>
    Enumerable.Range(0, StepCount2 + 1).Count(i => !IsPrime(RangeStart2 + i * Step2));

private static bool IsPrime(long n)
{
    InitPrimes(minMax: n);
    
    return PrimeSet.Contains(n);
}

private static void InitPrimes(long minMax)
{
    for (long i = PrimeList.Last(); i <= minMax; i += 2)
    {
        if (PrimeList.TakeWhile(p => p <= Math.Sqrt(i)).Any(p => i % p == 0))
        {
            continue;
        }
        
        PrimeList.Add(i);
        PrimeSet.Add(i);
    }
}

private static Action<Program>[] ParseInput(IEnumerable<string> input) =>
    input
        .Select<string, Action<Program>>(line =>
        {
            string[] lineParts = line.Split(' ');
            string cmdType = lineParts[0];
            string arg1 = lineParts[1];
            string? arg2 = (lineParts.Length > 2 ? lineParts[2] : null);
            
            return cmdType switch
            {
                "set" => p => p.Set(arg1, arg2!),
                "sub" => p => p.Sub(arg1, arg2!),
                "mul" => p => p.Mul(arg1, arg2!),
                "jnz" => p => p.Jnz(arg1, arg2!),
                _ => throw new InvalidOperationException($"Unknown command type: {cmdType}")
            };
        })
        .ToArray();

private class Program
{
    private readonly Dictionary<string, long> registers = [];
    
    public long LastInstruction { get; set; } = -1;
    
    public int MulCount { get; private set; } = 0;
    
    public void Set(string arg1, string arg2) =>
        this.SetReg(name: arg1, value: this.GetValue(arg2));
    
    public void Sub(string arg1, string arg2) =>
        this.SetReg(name: arg1, value: this.GetReg(arg1) - this.GetValue(arg2));
    
    public void Mul(string arg1, string arg2)
    {
        this.MulCount++;
        this.SetReg(name: arg1, value: this.GetReg(arg1) * this.GetValue(arg2));
    }
    
    public void Jnz(string arg1, string arg2)
    {
        if (this.GetValue(arg1) is 0)
        {
            return;
        }
    
        this.LastInstruction += this.GetValue(arg2!) - 1;
    }
    
    private void SetReg(string name, long value) =>
        registers[name] = value;
        
    private long GetValue(string arg) =>
        (long.TryParse(arg, out long value) ? value : this.GetReg(arg));
    
    private long GetReg(string name) =>
        (registers.TryGetValue(name, out long value) ? value : 0);
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
