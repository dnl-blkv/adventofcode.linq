<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyDictionary<(char Guest1, char Guest2), int> input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

const char Yourself = 'Y';
   
private static int Solve1(IReadOnlyDictionary<(char Guest1, char Guest2), int> input) => Solve(input);

private static int Solve2(IReadOnlyDictionary<(char Guest1, char Guest2), int> originalInput)
{
    var newInput = new Dictionary<(char Guest1, char Guest2), int>(originalInput);
    
    foreach (char guest in GetGuests(originalInput))
    {
        newInput[OrderPair(guest, Yourself)] = 0;
    }
    
    return Solve(newInput);
}
   
private static int Solve(IReadOnlyDictionary<(char Guest1, char Guest2), int> input)
{
    char[] guests = GetGuests(input).ToArray();
    
    return GetAllPermutations(guests[1..])
        .Select(permutations => GetCircularPairs(permutations.Prepend(guests[0])).Select(p => input[OrderPair(p)]).Sum())
        .Max();
}
    
private static IEnumerable<char> GetGuests(IReadOnlyDictionary<(char Guest1, char Guest2), int> input) =>
    input.Keys.SelectMany(k => new char[] { k.Guest1, k.Guest2 }).Distinct();
    
private static IEnumerable<IEnumerable<T>> GetAllPermutations<T>(T[] values, int startAt = 0)
{
    int currentCount = values.Length - startAt;

    if (currentCount == 1)
    {
        yield return Enumerable.Empty<T>().Append(values[startAt]);
    }
    
    for (int i = 0; i < currentCount; i++)
    {
        foreach (IEnumerable<T> permutation in GetAllPermutations(values, startAt + 1))
        {
            yield return InsertAt(permutation, values[startAt], i);
        }
    }
}    

private static IEnumerable<T> InsertAt<T>(IEnumerable<T> enumerable, T newItem, int newItemIndex)
{
    if (newItemIndex == 0)
    {
        yield return newItem;
    }
    
    int i = 1;
    
    foreach (T item in enumerable)
    {
        yield return item;
        
        if (i++ == newItemIndex)
        {
            yield return newItem;
        }
    }
}

private static IEnumerable<(T, T)> GetCircularPairs<T>(IEnumerable<T> items)
{
    T? firstItem = default;
    bool firstItemSet = false;
    T? previousItem = default;
    bool previousItemSet = false;
    
    foreach (T item in items)
    {
        if (!firstItemSet)
        {
            firstItem = item;
            firstItemSet = true;
        }
    
        if (previousItemSet)
        {
            yield return (previousItem!, item);
        }
        
        previousItem = item;
        previousItemSet = true;
    }
    
    if (firstItemSet && previousItemSet)
    {
        yield return (previousItem!, firstItem!);
    }
}

private static (T, T) OrderPair<T>((T, T) pair) => OrderPair(pair.Item1, pair.Item2);

private static (T, T) OrderPair<T>(T item1, T item2)
{
    T[] items =
    {
        item1,
        item2
    };
    Array.Sort(items);
    
    return (items[0], items[1]);
}

private static IReadOnlyDictionary<(char Guest1, char Guest2), int> ParseInput(IEnumerable<string> input)
{
    var parsed = new Dictionary<(char Guest1, char Guest2), int>();
    
    foreach (string line in input)
    {
        string[] lineParts = line.TrimEnd('.').Split(' ');
        (char Guest1, char Guest2) key = OrderPair(lineParts[0][0], lineParts[10][0]);
        
        if (!parsed.ContainsKey(key))
        {
            parsed[key] = 0;
        }
        
        parsed[key] += (lineParts[2] == "gain" ? 1 : -1) * int.Parse(lineParts[3]);
    }
    
    return parsed;
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}