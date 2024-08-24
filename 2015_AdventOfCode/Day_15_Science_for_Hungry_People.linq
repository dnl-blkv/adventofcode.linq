<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<FoodPart> input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private const int IngredientCountAllowed = 100;
private const int PropsCount = 4;

private static long Solve1(IReadOnlyList<FoodPart> input) => Solve(input, _ => true);

private static long Solve2(IReadOnlyList<FoodPart> input) => Solve(input, foodPart => foodPart.Calories == 500);

private static long Solve(IReadOnlyList<FoodPart> input, Func<FoodPart, bool> checkExtraCondition)
{
    long bestResult = -1;

    foreach (IEnumerable<int> shares in SplitShares(IngredientCountAllowed, PropsCount))
    {
        FoodPart currentTotal =
            input
                .Zip(shares, (share, part) => (share, part))
                .Aggregate(new FoodPart(), (result, next) => result.Merge(next.share, next.part));
        
        if (!checkExtraCondition(currentTotal))
        {
            continue;
        }
        
        bestResult = Math.Max(bestResult, currentTotal.TotalScore);
    }
    
    return bestResult;
}

private static IEnumerable<IEnumerable<int>> SplitShares(int resource, int receivers)
{
    if (receivers == 1)
    {
        yield return Enumerable.Empty<int>().Append(resource);
        yield break;
    }
    
    for (int i = 0; i < resource; i++)
    {
        foreach (IEnumerable<int> terms in SplitShares(resource - i, receivers - 1))
        {
            yield return terms.Prepend(i);
        }
    }
}

private static IReadOnlyList<FoodPart> ParseInput(IEnumerable<string> input) =>
    input.Select(FoodPart.Parse).ToArray();

private class FoodPart
{
    private readonly IReadOnlyList<int> props;

    public FoodPart(IReadOnlyList<int>? props = null, int calories = 0)
    {   
        props ??= Enumerable.Repeat(0, PropsCount).ToArray();
    
        this.props =
            props.Count == PropsCount
                ? props
                : throw new ArgumentException($"{nameof(props)} should have length of {PropsCount}", nameof(props));
        this.Calories = calories;
    }
    
    public int Calories { get; }
    
    public long TotalScore => this.props.Aggregate(1L, (t, p) => t * Math.Max(0, p));
    
    public static FoodPart Parse(string foodPartString)
    {
        int[] foodPartValues =
            foodPartString.Split(' ')
                .Select(s => (int.TryParse(s.Trim(','), out int n), n))
                .Where(t => t.Item1)
                .Select(t => t.Item2)
                .ToArray();
                
        return new FoodPart(props: foodPartValues[0..PropsCount], calories: foodPartValues[PropsCount]);
    }
        
    public FoodPart Merge(FoodPart other, int multiplier) =>
        new FoodPart(
            props: this.props.Zip(other.props, (p1, p2) => p1 + p2 * multiplier).ToArray(),
            calories: this.Calories + other.Calories * multiplier);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}