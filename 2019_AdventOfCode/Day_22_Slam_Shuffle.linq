<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<(Technique Technique, BigInteger Arg)> input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private enum Technique
{
    DealIntoNewStack = 0,
    CutN = 1,
    DealWithIncrementN = 2
}

private const int LandgingSiteMinSize2 = 4_000_000;

private static readonly BigInteger CardToTrack1 = 2019;
private static readonly BigInteger DeckSize1 = 10_007;

private static readonly BigInteger PositionToTrack2 = 2020;
private static readonly BigInteger DeckSize2 = 119315717514047;
private static readonly BigInteger IterationCount2 = 101741582076661;

private static BigInteger Solve1(IReadOnlyList<(Technique Technique, BigInteger Arg)> input)
{
    (BigInteger i, BigInteger c) = Optimize(input, DeckSize1);
    
    return PositiveModulo(CardToTrack1 * i + c, DeckSize1);
}

private static BigInteger Solve2(IReadOnlyList<(Technique Technique, BigInteger Arg)> input)
{
    (BigInteger i, BigInteger c) = Iterate(input, deckSize: DeckSize2, iterationCount: IterationCount2);
    c = PositiveModulo(c - PositionToTrack2, DeckSize2);
    
    // k * i + c === 0 (mod m)
    return FindK(i: i, c: c, m: DeckSize2, landingSiteMinSize: LandgingSiteMinSize2);
}
    
private static BigInteger PositiveModulo(BigInteger n, BigInteger mod) => (n % mod + mod) % mod;

private static Terms Optimize(IEnumerable<(Technique Technique, BigInteger Arg)> input, BigInteger deckSize) =>
    input.Aggregate(
        Terms.Zero,
        (t, o) =>
        {
            (Technique technique, BigInteger arg) = o;
        
            return t with
            {
                I = PositiveModulo(
                    n: technique switch
                    {
                        Technique.DealIntoNewStack => -t.I,
                        Technique.CutN => t.I,
                        Technique.DealWithIncrementN => t.I * arg,
                        _ => throw CreateUnknownTechniqueException(technique)
                    },
                    deckSize),
                C = PositiveModulo(
                    n: technique switch
                    {
                        Technique.DealIntoNewStack => -t.C - 1,
                        Technique.CutN => t.C - arg,
                        Technique.DealWithIncrementN => t.C * arg,
                        _ => throw CreateUnknownTechniqueException(technique)
                    },
                    deckSize)
            };
        });

private static Exception CreateUnknownTechniqueException(Technique unknownTechnique) =>
    throw new InvalidOperationException($"Unknown technique: '{unknownTechnique}'.");

private static Terms Iterate(
    IEnumerable<(Technique Technique, BigInteger Arg)> input,
    BigInteger deckSize,
    BigInteger iterationCount)
{
    Terms result = Terms.Zero;
    Terms currentBitTerms = Optimize(input, DeckSize2);
    
    foreach (bool bit in GetBits(iterationCount))
    {
        if (bit)
        {
            result = result.Merge(currentBitTerms, deckSize);
        }
        
        currentBitTerms = currentBitTerms.Merge(currentBitTerms, deckSize);
    }
    
    return result;
}
        
private static IEnumerable<bool> GetBits(BigInteger n)
{
    BigInteger rem = n;
    
    while (rem > 0)
    {
        yield return rem % 2 == 1;
        rem /= 2;
    }
}

// Finds k in "k * i + c === 0 (mod m)"
private static BigInteger FindK(BigInteger i, BigInteger c, BigInteger m, int landingSiteMinSize)
{
    var landingSite =
        new Dictionary<BigInteger, BigInteger>
        {
            [0] = 0
        };
    BigInteger currentI = i;
    BigInteger currentJump = 1;
    
    while (landingSite.Count < landingSiteMinSize)
    {
        BigInteger qBoundary = (m + currentI - 1) / currentI;
    
        foreach (BigInteger rem in landingSite.Keys.ToList())
        {
            for (long q = 1; q < qBoundary; q++)
            {
                BigInteger newRem = PositiveModulo(rem + q * currentI, m);
                BigInteger newShiftBack = landingSite[rem] + q * currentJump;
                landingSite.Add(newRem, newShiftBack);
            }
        }
    
        currentI = PositiveModulo(currentI * qBoundary, m);
        currentJump *= qBoundary;
    }

    for (BigInteger k = 0;; k += landingSite.Count)
    {
        if (!landingSite.TryGetValue(PositiveModulo(k * i + c, m), out BigInteger shiftBack))
        {
            continue;
        }
        
        return k - shiftBack;
    }
}

private static IReadOnlyList<(Technique Technique, BigInteger Arg)> ParseInput(IEnumerable<string> input) =>
    input
        .Select(line =>
            BigInteger.TryParse(line[line.LastIndexOf(' ')..], out BigInteger arg)
                ? (Technique: (line[0] is 'c' ? Technique.CutN : Technique.DealWithIncrementN), Arg: arg)
                : (Technique: Technique.DealIntoNewStack, Arg: 0))
        .ToArray();
        
private readonly record struct Terms(BigInteger I, BigInteger C)
{
    public static readonly Terms Zero = new Terms(I: 1, C: 0);

    public Terms Merge(Terms other, BigInteger deckSize) =>
        this with
        {
            I = PositiveModulo(this.I * other.I, deckSize),
            C = PositiveModulo(this.C * other.I + other.C, deckSize)
        };
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
