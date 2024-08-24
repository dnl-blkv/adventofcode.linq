<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<AuntSue> input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static AuntSuePropMatcher[] V1PropMatchers =
    Enumerable.Empty<Func<AuntSue, int?>>()
        .Append(asue => asue.Children)
        .Append(asue => asue.Cats)
        .Append(asue => asue.Samoyeds)
        .Append(asue => asue.Pomeranians)
        .Append(asue => asue.Akitas)
        .Append(asue => asue.Vizslas)
        .Append(asue => asue.Goldfish)
        .Append(asue => asue.Trees)
        .Append(asue => asue.Cars)
        .Append(asue => asue.Perfumes)
        .Select(getter => new AuntSuePropMatcher(getter, EqualMatch))
        .ToArray();

private static AuntSuePropMatcher[] V2PropMatchers =
    V1PropMatchers
        .Select((pm, i) =>
        {
            Func<int?, int?, bool> comparer = i switch
            {
                1 or 7 => GreaterThanMatch,
                3 or 6 => LessThanMatch,
                _ => EqualMatch
            };
        
            return new AuntSuePropMatcher(pm.Getter, comparer);
        })
        .ToArray();
        
private static AuntSue TargetAuntSuePattern =
    new()
    {
        Children = 3,
        Cats = 7,
        Samoyeds = 2,
        Pomeranians = 3,
        Akitas = 0,
        Vizslas = 0,
        Goldfish = 5,
        Trees = 3,
        Cars = 2,
        Perfumes = 1
    };

private static bool EqualMatch(int? a, int? b) => a == b;

private static bool GreaterThanMatch(int? a, int? b) => a > b;

private static bool LessThanMatch(int? a, int? b) => a < b;

private static int Solve1(IReadOnlyList<AuntSue> input) => Solve(input, V1PropMatchers);

private static int Solve2(IReadOnlyList<AuntSue> input) => Solve(input, V2PropMatchers);

private static int Solve(IReadOnlyList<AuntSue> input, IEnumerable<AuntSuePropMatcher> propMatchers) =>
    input
        .Select((aunt, number) => (aunt, number))
        .First(p => propMatchers.All(matcher => matcher.IsMatch(p.aunt, TargetAuntSuePattern))).number
        + 1;

private static IReadOnlyList<AuntSue> ParseInput(IEnumerable<string> input) =>
    input.Select(AuntSue.Parse).ToArray();

private class AuntSue
{
    public int? Children { get; init; } = null;
    
    public int? Cats { get; init; } = null;
    
    public int? Samoyeds { get; init; } = null;
    
    public int? Pomeranians { get; init; } = null;
    
    public int? Akitas { get; init; } = null;
    
    public int? Vizslas { get; init; } = null;
    
    public int? Goldfish { get; init; } = null;
    
    public int? Trees { get; init; } = null;
    
    public int? Cars { get; init; } = null;
    
    public int? Perfumes { get; init; } = null;
    
    public static AuntSue Parse(string auntSueString)
    {
        string[] auntSueStringParts = auntSueString.Split(": ", 2);
        string[] auntSueProps = auntSueStringParts[1].Split(", ");
        var auntSueDict =
            auntSueProps
                .Select(prop => prop.Split(": "))
                .ToDictionary(kv => kv[0], kv => int.Parse(kv[1]));
        
        return new AuntSue
        {
            Children = GetPropOrNull(nameof(Children)),
            Cats = GetPropOrNull(nameof(Cats)),
            Samoyeds = GetPropOrNull(nameof(Samoyeds)),
            Pomeranians = GetPropOrNull(nameof(Pomeranians)),
            Akitas = GetPropOrNull(nameof(Akitas)),
            Vizslas = GetPropOrNull(nameof(Vizslas)),
            Goldfish = GetPropOrNull(nameof(Goldfish)),
            Trees = GetPropOrNull(nameof(Trees)),
            Cars = GetPropOrNull(nameof(Cars)),
            Perfumes = GetPropOrNull(nameof(Perfumes))
        };
        
        int? GetPropOrNull(string propName) =>
            auntSueDict.TryGetValue(propName.ToLower(), out int prop)
                ? prop
                : null;
    }
}

private class AuntSuePropMatcher
{
    public AuntSuePropMatcher(Func<AuntSue, int?> getter, Func<int?, int?, bool> comparer)
    {
        this.Getter = getter;
        this.Comparer = comparer;
    }
    
    public Func<AuntSue, int?> Getter { get; }
    
    public Func<int?, int?, bool> Comparer { get; }

    public bool IsMatch(AuntSue input, AuntSue pattern) =>
        this.Getter(input) is null || this.Comparer(this.Getter(input), this.Getter(pattern));
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}