<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IpV7[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static long Solve1(IpV7[] input) =>
    input.Count(ipV7 => ipV7.SupportsTls());

private static long Solve2(IpV7[] input) =>
    input.Count(ipV7 => ipV7.SupportsSsl());

private static IpV7[] ParseInput(IEnumerable<string> input) =>
    input.Select(IpV7.Parse).ToArray();

private record struct IpV7(IReadOnlyList<string> SupernetSequences, IReadOnlyList<string> HypernetSequences)
{
    private static char[] Braces = [ '[', ']' ];

    public static IpV7 Parse(string ipV7String)
    {
        IReadOnlyDictionary<int, string[]> sequences =
            ipV7String
                .Split(Braces, StringSplitOptions.RemoveEmptyEntries)
                .Select((p, i) => (P: p, I: i))
                .GroupBy(t => t.I % 2)
                .ToDictionary(g => g.Key, g => g.Select(t => t.P).ToArray());
        
        return new IpV7(SupernetSequences: sequences[0], HypernetSequences: sequences[1]);
    }
    
    public bool SupportsTls() =>
        this.SupernetSequences.Any(HasAbba) && !this.HypernetSequences.Any(HasAbba);
        
    public bool SupportsSsl()
    {
        HashSet<string> babCandidates = this.SupernetSequences.SelectMany(GetBabCandidates).ToHashSet();
    
        return this.HypernetSequences.Any(hs => babCandidates.Any(hs.Contains));
    }
    
    private static bool HasAbba(string str)
    {
        for (int i = 0; i < str.Length - 3; i++)
        {
            if (str[i] == str[i + 1] || str[i] != str[i + 3] || str[i + 1] != str[i + 2])
            {
                continue;
            }
            
            return true;
        }
        
        return false;
    }
    
    private static IEnumerable<string> GetBabCandidates(string str)
    {
        for (int i = 0; i < str.Length - 2; i++)
        {
            if (str[i] == str[i + 1] || str[i] != str[i + 2])
            {
                continue;
            }
            
            yield return $"{str[i + 1]}{str[i]}{str[i + 1]}";
        }
    }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}