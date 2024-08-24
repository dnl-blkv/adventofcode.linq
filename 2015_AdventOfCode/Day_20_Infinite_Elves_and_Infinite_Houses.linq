<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    int input = ParseInput(GetInput());
    Primes primes = Primes.Create((int)Math.Sqrt(input / Math.Min(InputDivisor1, InputDivisor2)));
	Solve1(input, primes).Dump();
	Solve2(input, primes).Dump();
}

private const int InputDivisor1 = 10;
private const int InputDivisor2 = 11;

private static int Solve1(int input, Primes primes) =>
    Solve(input, primes, InputDivisor1, shouldUseDivisor: (_, _) => true);

private static int Solve2(int input, Primes primes) =>
    Solve(input, primes, InputDivisor2, shouldUseDivisor: (i, div) => i / div <= 50);

private static int Solve(int input, Primes primes, double inputDivisor, Func<int, int, bool> shouldUseDivisor)
{
    int target = (int)Math.Ceiling(input / inputDivisor);

    for (int i = 2; i < target; i++)
    {
        if (FindAllDivisors(i, primes).Where(div => shouldUseDivisor(i, div)).Sum() < target)
        {
            continue;
        }
        
        return i;
    }
    
    return -1;
}
    
private static IEnumerable<int> FindAllDivisors(int value, Primes primes)
{
    var divisors = new HashSet<int>
    {
        1
    };
    
    foreach (int primeDivisor in primes.FindAllPrimeDivisors(value))
    {
        divisors.UnionWith(divisors.Select(div => div * primeDivisor).ToArray());
    }
    
    return divisors;
}

private static int ParseInput(IEnumerable<string> input) => int.Parse(input.Single());

private class Primes
{
    private readonly IReadOnlyList<int> list;
 
    private Primes(IReadOnlyList<int> list) => this.list = list;
    
    public static Primes Create(int maxValue)
    {
        var primes = new List<int>
        {
            2
        };

        for (int i = 3; i <= maxValue; i += 2)
        {
            int cutOff = (int)Math.Sqrt(i);
            
            if (primes.TakeWhile(p => p <= cutOff).Any(p => i % p == 0))
            {
                continue;
            }
            
            primes.Add(i);
        }
        
        return new Primes(list: primes);
    }
    
    public IEnumerable<int> FindAllPrimeDivisors(int value)
    {
        int currentValue = value;
        int largestPossiblePrimeDivisor = (int)(Math.Sqrt(value));
    
        for (int i = 0; this.list[i] <= largestPossiblePrimeDivisor; i++)
        {
            while (currentValue % this.list[i] == 0)
            {
                yield return this.list[i];
                currentValue /= this.list[i];
            }
        }
    }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}